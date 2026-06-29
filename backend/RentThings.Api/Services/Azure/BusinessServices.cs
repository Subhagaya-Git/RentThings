using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.DTOs;
using RentThings.Api.Models;
using RentThings.Api.Services;
using RentThings.Api.Services.Azure;

namespace RentThings.Api.Services;

public interface IListingService
{
    Task<PagedResult<ListingDto>> SearchAsync(ListingSearchParams p, CancellationToken ct = default);
    Task<ListingDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ListingDto> CreateAsync(Guid ownerId, CreateListingRequest req, CancellationToken ct = default);
    Task<ListingDto?> UpdateAsync(Guid id, Guid ownerId, CreateListingRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken ct = default);
    Task<ListingImageDto> AddImageAsync(Guid listingId, Guid ownerId, Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<bool> DeleteImageAsync(Guid listingId, Guid imageId, Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<ListingDto>> GetFeaturedAsync(int count = 6, CancellationToken ct = default);
    Task<IReadOnlyList<ListingDto>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<OwnerDashboardDto> GetOwnerDashboardAsync(Guid ownerId, CancellationToken ct = default);
    Task<bool> SetListingStatusAsync(Guid id, ListingStatus status, CancellationToken ct = default);
}

public class ListingService(
    RentThingsDbContext db,
    IBlobStorageService blobStorage,
    IAiVisionService vision,
    IMapsService maps,
    INotificationPublisher notifications) : IListingService
{
    public async Task<PagedResult<ListingDto>> SearchAsync(ListingSearchParams p, CancellationToken ct = default)
    {
        var query = db.Listings
            .Include(l => l.Owner)
            .Include(l => l.Images)
            .Include(l => l.Availability)
            .Where(l => l.Status == ListingStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(p.Query))
            query = query.Where(l => l.Title.Contains(p.Query) || l.Description.Contains(p.Query) || l.Category.Contains(p.Query));

        if (!string.IsNullOrWhiteSpace(p.Category))
            query = query.Where(l => l.Category == p.Category);

        if (!string.IsNullOrWhiteSpace(p.Location))
            query = query.Where(l => l.Location.Contains(p.Location) || (l.City != null && l.City.Contains(p.Location)));

        if (p.MinPrice.HasValue) query = query.Where(l => l.PricePerDay >= p.MinPrice);
        if (p.MaxPrice.HasValue) query = query.Where(l => l.PricePerDay <= p.MaxPrice);
        if (p.MinRating.HasValue) query = query.Where(l => l.AverageRating >= p.MinRating);

        if (p.AvailableFrom.HasValue || p.AvailableTo.HasValue)
        {
            var from = p.AvailableFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var to = p.AvailableTo ?? from.AddDays(365);
            query = query.Where(l => l.Availability.Any(a => a.IsAvailable && a.Date >= from && a.Date <= to)
                || !l.Availability.Any());
        }

        var items = await query.ToListAsync(ct);

        if (p.Latitude.HasValue && p.Longitude.HasValue && p.RadiusKm.HasValue)
        {
            items = items
                .Where(l => l.Latitude.HasValue && l.Longitude.HasValue)
                .Where(l => maps.CalculateDistanceKm(p.Latitude.Value, p.Longitude.Value, l.Latitude!.Value, l.Longitude!.Value) <= p.RadiusKm.Value)
                .ToList();
        }

        IEnumerable<Listing> sorted = p.SortBy switch
        {
            "price_asc" => items.OrderBy(l => l.PricePerDay),
            "price_desc" => items.OrderByDescending(l => l.PricePerDay),
            "rating" => items.OrderByDescending(l => l.AverageRating),
            "newest" => items.OrderByDescending(l => l.CreatedAt),
            "distance" when p.Latitude.HasValue && p.Longitude.HasValue =>
                items.OrderBy(l => l.Latitude.HasValue && l.Longitude.HasValue
                    ? maps.CalculateDistanceKm(p.Latitude.Value, p.Longitude.Value, l.Latitude!.Value, l.Longitude!.Value)
                    : double.MaxValue),
            _ => items.OrderByDescending(l => l.IsFeatured).ThenByDescending(l => l.AverageRating)
        };

        var total = items.Count;
        var page = sorted.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToList();

        double? searchLat = p.Latitude;
        double? searchLon = p.Longitude;
        var dtos = page.Select(l =>
        {
            double? dist = searchLat.HasValue && searchLon.HasValue && l.Latitude.HasValue && l.Longitude.HasValue
                ? maps.CalculateDistanceKm(searchLat.Value, searchLon.Value, l.Latitude.Value, l.Longitude.Value)
                : null;
            return MapListing(l, dist);
        }).ToList();

        return new PagedResult<ListingDto>(dtos, total, p.Page, p.PageSize);
    }

    public async Task<ListingDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var listing = await db.Listings.Include(l => l.Owner).Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (listing is null) return null;
        listing.ViewCount++;
        await db.SaveChangesAsync(ct);
        return MapListing(listing, maps: maps);
    }

    public async Task<ListingDto> CreateAsync(Guid ownerId, CreateListingRequest req, CancellationToken ct = default)
    {
        var geocode = await maps.GeocodeAsync(req.Location, ct);
        var listing = new Listing
        {
            OwnerId = ownerId,
            Title = req.Title,
            Description = req.Description,
            Category = req.Category,
            PricePerDay = req.PricePerDay,
            Deposit = req.Deposit,
            Location = req.Location,
            City = req.City,
            State = req.State,
            Latitude = geocode?.Latitude,
            Longitude = geocode?.Longitude,
            Status = ListingStatus.Active
        };
        db.Listings.Add(listing);
        await db.SaveChangesAsync(ct);

        if (req.AvailableFrom.HasValue && req.AvailableTo.HasValue)
            await SetAvailabilityRangeAsync(listing.Id, req.AvailableFrom.Value, req.AvailableTo.Value, ct);

        await db.Entry(listing).Reference(l => l.Owner).LoadAsync(ct);
        var dto = MapListing(listing, maps: maps);
        await notifications.PublishListingCreatedAsync(dto, ct);
        return dto;
    }

    static async Task SetAvailabilityRangeAsync(RentThingsDbContext db, Guid listingId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            db.ListingAvailability.Add(new ListingAvailability
            {
                ListingId = listingId,
                Date = d,
                IsAvailable = true
            });
        }
        await db.SaveChangesAsync(ct);
    }

    async Task SetAvailabilityRangeAsync(Guid listingId, DateOnly from, DateOnly to, CancellationToken ct)
        => await SetAvailabilityRangeAsync(db, listingId, from, to, ct);

    public async Task<ListingDto?> UpdateAsync(Guid id, Guid ownerId, CreateListingRequest req, CancellationToken ct = default)
    {
        var listing = await db.Listings.Include(l => l.Owner).Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == ownerId, ct);
        if (listing is null) return null;

        listing.Title = req.Title;
        listing.Description = req.Description;
        listing.Category = req.Category;
        listing.PricePerDay = req.PricePerDay;
        listing.Deposit = req.Deposit;
        listing.Location = req.Location;
        listing.City = req.City;
        listing.State = req.State;
        var geocode = await maps.GeocodeAsync(req.Location, ct);
        if (geocode is not null)
        {
            listing.Latitude = geocode.Latitude;
            listing.Longitude = geocode.Longitude;
        }
        listing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        if (req.AvailableFrom.HasValue && req.AvailableTo.HasValue)
        {
            var existing = await db.ListingAvailability.Where(a => a.ListingId == id).ToListAsync(ct);
            db.ListingAvailability.RemoveRange(existing);
            await db.SaveChangesAsync(ct);
            await SetAvailabilityRangeAsync(id, req.AvailableFrom.Value, req.AvailableTo.Value, ct);
        }

        var dto = MapListing(listing, maps: maps);
        await notifications.PublishListingUpdatedAsync(dto, ct);
        return dto;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken ct = default)
    {
        var listing = await db.Listings.FirstOrDefaultAsync(l => l.Id == id && l.OwnerId == ownerId, ct);
        if (listing is null) return false;

        var hasActiveRentals = await db.Rentals.AnyAsync(r =>
            r.ListingId == id && (r.Status == RentalStatus.Requested || r.Status == RentalStatus.Approved
                || r.Status == RentalStatus.Active || r.Status == RentalStatus.HandedOver), ct);
        if (hasActiveRentals)
            throw new InvalidOperationException("Cannot deactivate listing with active rentals.");

        listing.Status = ListingStatus.Inactive;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ListingImageDto> AddImageAsync(Guid listingId, Guid ownerId, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var listing = await db.Listings.FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId, ct)
            ?? throw new InvalidOperationException("Listing not found.");

        var validation = await vision.ValidateListingImageAsync(stream, ct);
        stream.Position = 0;
        var url = await blobStorage.UploadListingImageAsync(stream, fileName, contentType, ct);
        var thumb = await blobStorage.GetOptimizedThumbnailUrlAsync(url, ct);

        var image = new ListingImage
        {
            ListingId = listingId,
            BlobUrl = url,
            ThumbnailUrl = thumb,
            SortOrder = await db.ListingImages.CountAsync(i => i.ListingId == listingId, ct),
            IsPrimary = !await db.ListingImages.AnyAsync(i => i.ListingId == listingId, ct),
            PassedValidation = validation.IsValid,
            ValidationNotes = validation.Issues.Any() ? string.Join("; ", validation.Issues) : null
        };

        db.ListingImages.Add(image);

        if (!string.IsNullOrWhiteSpace(validation.Category))
            listing.Category = validation.Category;

        await db.SaveChangesAsync(ct);

        await db.Entry(listing).Reference(l => l.Owner).LoadAsync(ct);
        await db.Entry(listing).Collection(l => l.Images).LoadAsync(ct);
        await notifications.PublishListingUpdatedAsync(MapListing(listing, maps: maps), ct);

        return new ListingImageDto(image.Id, image.BlobUrl, image.ThumbnailUrl, image.IsPrimary, image.PassedValidation, image.ValidationNotes);
    }

    public async Task<bool> DeleteImageAsync(Guid listingId, Guid imageId, Guid ownerId, CancellationToken ct = default)
    {
        var listing = await db.Listings.FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId, ct);
        if (listing is null) return false;

        var image = await db.ListingImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ListingId == listingId, ct);
        if (image is null) return false;

        var wasPrimary = image.IsPrimary;
        db.ListingImages.Remove(image);
        await db.SaveChangesAsync(ct);

        if (wasPrimary)
        {
            var next = await db.ListingImages.Where(i => i.ListingId == listingId).OrderBy(i => i.SortOrder).FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsPrimary = true;
                await db.SaveChangesAsync(ct);
            }
        }
        return true;
    }

    public async Task<IReadOnlyList<ListingDto>> GetFeaturedAsync(int count = 6, CancellationToken ct = default)
    {
        var items = await db.Listings.Include(l => l.Owner).Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Active && l.IsFeatured)
            .OrderByDescending(l => l.AverageRating)
            .Take(count)
            .ToListAsync(ct);
        return items.Select(l => MapListing(l, maps: maps)).ToList();
    }

    public async Task<IReadOnlyList<ListingDto>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        var items = await db.Listings.Include(l => l.Owner).Include(l => l.Images)
            .Where(l => l.OwnerId == ownerId && l.Status != ListingStatus.Inactive)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
        return items.Select(l => MapListing(l, maps: maps)).ToList();
    }

    public async Task<OwnerDashboardDto> GetOwnerDashboardAsync(Guid ownerId, CancellationToken ct = default)
    {
        var listings = await db.Listings.Include(l => l.Owner).Include(l => l.Images)
            .Where(l => l.OwnerId == ownerId).OrderByDescending(l => l.CreatedAt).ToListAsync(ct);

        var rentals = await db.Rentals.Include(r => r.Listing).ThenInclude(l => l.Images).Include(r => r.Renter)
            .Where(r => r.Listing.OwnerId == ownerId).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

        var activeStatuses = new[] { RentalStatus.Approved, RentalStatus.Active, RentalStatus.HandedOver };
        var completedStatuses = new[] { RentalStatus.Completed, RentalStatus.Reviewed };

        return new OwnerDashboardDto(
            listings.Count(l => l.Status == ListingStatus.Active),
            listings.Count(l => l.Status == ListingStatus.Inactive),
            rentals.Count(r => r.Status == RentalStatus.Requested),
            rentals.Count(r => activeStatuses.Contains(r.Status)),
            rentals.Where(r => completedStatuses.Contains(r.Status)).Sum(r => r.TotalPrice),
            listings.Select(l => MapListing(l, maps: maps)).ToList(),
            rentals.Select(r => RentalService.MapRental(r, r.Listing)).ToList(),
            rentals.Where(r => activeStatuses.Contains(r.Status)).Select(r => RentalService.MapRental(r, r.Listing)).ToList());
    }

    public async Task<bool> SetListingStatusAsync(Guid id, ListingStatus status, CancellationToken ct = default)
    {
        var listing = await db.Listings.FindAsync([id], ct);
        if (listing is null) return false;
        listing.Status = status;
        listing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var updated = await db.Listings.Include(l => l.Owner).Include(l => l.Images)
            .FirstAsync(l => l.Id == id, ct);
        await notifications.PublishListingUpdatedAsync(MapListing(updated, maps: maps), ct);
        return true;
    }

    internal static ListingDto MapListing(Listing l, double? distanceKm = null, IMapsService? maps = null)
    {
        string? mapUrl = l.Latitude.HasValue && l.Longitude.HasValue && maps is not null
            ? maps.GetStaticMapUrl(l.Latitude.Value, l.Longitude.Value)
            : null;

        return new ListingDto(
            l.Id, l.OwnerId, $"{l.Owner.FirstName} {l.Owner.LastName}", l.Title, l.Description,
            l.Category, l.PricePerDay, l.Deposit, l.Location, l.City, l.State,
            l.Status.ToString(), l.AverageRating, l.ReviewCount, l.IsFeatured,
            l.Images.OrderBy(i => i.SortOrder).Select(i => new ListingImageDto(
                i.Id, i.BlobUrl, i.ThumbnailUrl, i.IsPrimary, i.PassedValidation, i.ValidationNotes)).ToList(),
            l.CreatedAt, l.Latitude, l.Longitude, distanceKm, mapUrl);
    }
}

public interface IRentalService
{
    Task<RentalDto> CreateRequestAsync(Guid renterId, CreateRentalRequest req, CancellationToken ct = default);
    Task<RentalDto?> UpdateStatusAsync(Guid id, Guid userId, UpdateRentalStatusRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<RentalDto>> GetByRenterAsync(Guid renterId, CancellationToken ct = default);
    Task<IReadOnlyList<RentalDto>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<RentalDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public class RentalService(
    RentThingsDbContext db,
    ICommunicationService comm,
    ITrustScoreService trust,
    INotificationPublisher notifications) : IRentalService
{
    public async Task<RentalDto> CreateRequestAsync(Guid renterId, CreateRentalRequest req, CancellationToken ct = default)
    {
        var listing = await db.Listings.Include(l => l.Owner).FirstOrDefaultAsync(l => l.Id == req.ListingId, ct)
            ?? throw new InvalidOperationException("Listing not found.");

        var days = req.EndDate.DayNumber - req.StartDate.DayNumber + 1;
        if (days < 1) throw new InvalidOperationException("Invalid date range.");

        var rental = new Rental
        {
            ListingId = req.ListingId,
            RenterId = renterId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalPrice = listing.PricePerDay * days,
            DepositAmount = listing.Deposit,
            Message = req.Message,
            Status = RentalStatus.Requested
        };

        db.Rentals.Add(rental);
        var ownerNotif = new Notification
        {
            UserId = listing.OwnerId,
            Type = NotificationType.BookingRequest,
            Title = "New rental request",
            Message = $"New request for {listing.Title}",
            ActionUrl = $"/owner/requests/{rental.Id}"
        };
        db.Notifications.Add(ownerNotif);
        await db.SaveChangesAsync(ct);

        await notifications.PublishAsync(listing.OwnerId, NotificationMapper.ToDto(ownerNotif), ct);

        await db.Entry(rental).Reference(r => r.Renter).LoadAsync(ct);
        return MapRental(rental, listing);
    }

    public async Task<RentalDto?> UpdateStatusAsync(Guid id, Guid userId, UpdateRentalStatusRequest req, CancellationToken ct = default)
    {
        var rental = await db.Rentals
            .Include(r => r.Listing).ThenInclude(l => l.Owner)
            .Include(r => r.Renter)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rental is null) return null;

        if (rental.Listing.OwnerId != userId && rental.RenterId != userId)
            throw new UnauthorizedAccessException();

        if (!Enum.TryParse<RentalStatus>(req.Status, true, out var status))
            throw new InvalidOperationException("Invalid status.");

        var isOwner = rental.Listing.OwnerId == userId;
        var isRenter = rental.RenterId == userId;
        ValidateStatusTransition(rental.Status, status, isOwner, isRenter);

        rental.Status = status;
        if (status == RentalStatus.Rejected && !string.IsNullOrWhiteSpace(req.Notes))
            rental.OwnerNotes = req.Notes;
        else if (status != RentalStatus.Rejected)
            rental.OwnerNotes = req.Notes ?? rental.OwnerNotes;
        rental.UpdatedAt = DateTime.UtcNow;

        if (status == RentalStatus.Approved) rental.ApprovedAt = DateTime.UtcNow;
        if (status is RentalStatus.Completed or RentalStatus.Reviewed)
        {
            rental.CompletedAt = DateTime.UtcNow;
            await trust.RecalculateAsync(rental.RenterId, ct);
            await trust.RecalculateAsync(rental.Listing.OwnerId, ct);
        }

        var notifType = status switch
        {
            RentalStatus.Approved => NotificationType.BookingApproved,
            RentalStatus.Rejected => NotificationType.BookingRejected,
            RentalStatus.Completed => NotificationType.RentalCompleted,
            _ => NotificationType.System
        };

        var renterNotif = new Notification
        {
            UserId = rental.RenterId,
            Type = notifType,
            Title = $"Rental {status}",
            Message = $"Your rental for {rental.Listing.Title} is now {status.ToString().ToLower()}.",
            ActionUrl = $"/dashboard/rentals/{rental.Id}"
        };
        db.Notifications.Add(renterNotif);

        await db.SaveChangesAsync(ct);

        await notifications.PublishAsync(rental.RenterId, NotificationMapper.ToDto(renterNotif), ct);

        if (status == RentalStatus.Approved)
        {
            await comm.SendBookingConfirmationAsync(rental.Renter.Email, rental.Listing.Title, rental.StartDate, rental.EndDate, ct);
            if (!string.IsNullOrWhiteSpace(rental.Renter.Phone))
                await comm.SendBookingApprovedSmsAsync(rental.Renter.Phone, rental.Listing.Title, rental.StartDate, rental.EndDate, ct);
        }
        else if (status == RentalStatus.Rejected && !string.IsNullOrWhiteSpace(rental.Renter.Phone))
        {
            await comm.SendBookingRejectedSmsAsync(rental.Renter.Phone, rental.Listing.Title, ct);
        }

        // Notify owner on completion + review events handled elsewhere
        if (status == RentalStatus.Completed && !string.IsNullOrWhiteSpace(rental.Renter.Phone))
        {
            var returnDate = rental.EndDate.AddDays(1);
            if (returnDate == DateOnly.FromDateTime(DateTime.UtcNow))
                await comm.SendReturnReminderSmsAsync(rental.Renter.Phone, rental.Listing.Title, rental.EndDate, ct);
        }

        return MapRental(rental, rental.Listing);
    }

    static void ValidateStatusTransition(RentalStatus current, RentalStatus next, bool isOwner, bool isRenter)
    {
        var allowed = (current, next) switch
        {
            (RentalStatus.Requested, RentalStatus.Approved) => isOwner,
            (RentalStatus.Requested, RentalStatus.Rejected) => isOwner,
            (RentalStatus.Approved, RentalStatus.HandedOver) => isOwner,
            (RentalStatus.Approved, RentalStatus.Active) => isOwner,
            (RentalStatus.HandedOver, RentalStatus.Returned) => isRenter,
            (RentalStatus.Active, RentalStatus.Returned) => isRenter,
            (RentalStatus.Returned, RentalStatus.Reviewed) => isOwner || isRenter,
            (RentalStatus.Returned, RentalStatus.Completed) => isOwner || isRenter,
            (_, RentalStatus.Cancelled) => isOwner || isRenter,
            _ when current == next => true,
            _ => false
        };
        if (!allowed)
            throw new InvalidOperationException($"Cannot transition from {current} to {next}.");
    }

    public async Task<IReadOnlyList<RentalDto>> GetByRenterAsync(Guid renterId, CancellationToken ct = default)
    {
        var rentals = await db.Rentals.Include(r => r.Listing).ThenInclude(l => l.Owner).Include(r => r.Listing).ThenInclude(l => l.Images)
            .Include(r => r.Renter).Where(r => r.RenterId == renterId).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return rentals.Select(r => MapRental(r, r.Listing)).ToList();
    }

    public async Task<IReadOnlyList<RentalDto>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        var rentals = await db.Rentals.Include(r => r.Listing).ThenInclude(l => l.Images).Include(r => r.Renter)
            .Where(r => r.Listing.OwnerId == ownerId).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return rentals.Select(r => MapRental(r, r.Listing)).ToList();
    }

    public async Task<RentalDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rental = await db.Rentals.Include(r => r.Listing).ThenInclude(l => l.Owner).Include(r => r.Listing).ThenInclude(l => l.Images)
            .Include(r => r.Renter).FirstOrDefaultAsync(r => r.Id == id, ct);
        return rental is null ? null : MapRental(rental, rental.Listing);
    }

    internal static RentalDto MapRental(Rental r, Listing l) => new(
        r.Id, l.Id, l.Title, l.Images.FirstOrDefault(i => i.IsPrimary)?.BlobUrl ?? l.Images.FirstOrDefault()?.BlobUrl,
        r.RenterId, $"{r.Renter.FirstName} {r.Renter.LastName}",
        l.OwnerId, $"{l.Owner.FirstName} {l.Owner.LastName}",
        r.StartDate, r.EndDate, r.Status.ToString(), r.TotalPrice, r.DepositAmount, r.Message,
        r.Status == RentalStatus.Rejected ? r.OwnerNotes : null, r.CreatedAt);
}

public static class NotificationMapper
{
    public static NotificationDto ToDto(Notification n) => new(
        n.Id, n.Type.ToString(), n.Title, n.Message, n.ActionUrl, n.IsRead, n.CreatedAt);
}

public static class UserMapper
{
    public static UserDto Map(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.Bio, u.Location, u.AvatarUrl,
        u.Role.ToString(), u.TrustScore, u.TrustLevel.ToString(), u.IsVerified, u.IsActive, u.CreatedAt);
}
