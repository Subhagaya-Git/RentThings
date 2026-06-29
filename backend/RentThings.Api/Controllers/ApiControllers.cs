using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.DTOs;
using RentThings.Api.Models;
using RentThings.Api.Services;
using RentThings.Api.Services.Azure;
using System.Security.Claims;

namespace RentThings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IEntraIdService entra) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<UserRole>(req.Role, true, out var role)) role = UserRole.Renter;
        var result = await entra.RegisterAsync(req.Email, req.Password, req.FirstName, req.LastName, role, ct);
        if (!result.Success) return BadRequest(new { error = result.Error });
        return Ok(new AuthResponse(result.Token!, UserMapper.Map(result.User!)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest req, CancellationToken ct)
    {
        var result = await entra.LoginAsync(req.Email, req.Password, ct);
        if (!result.Success) return Unauthorized(new { error = result.Error });
        return Ok(new AuthResponse(result.Token!, UserMapper.Map(result.User!)));
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> PasswordReset([FromBody] AuthRequest req, CancellationToken ct)
    {
        await entra.SendPasswordResetAsync(req.Email, ct);
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetProfile(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var user = await db.Users.FindAsync([userId.Value], ct);
        return user is null ? NotFound() : Ok(UserMapper.Map(user));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var user = await db.Users.FindAsync([userId.Value], ct);
        if (user is null) return NotFound();

        if (req.FirstName is not null) user.FirstName = req.FirstName;
        if (req.LastName is not null) user.LastName = req.LastName;
        if (req.Phone is not null) user.Phone = req.Phone;
        if (req.Bio is not null) user.Bio = req.Bio;
        if (req.Location is not null) user.Location = req.Location;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(UserMapper.Map(user));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct);
        return user is null ? NotFound() : Ok(UserMapper.Map(user));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

[ApiController]
[Route("api/[controller]")]
public class ListingsController(IListingService listings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ListingDto>>> Search([FromQuery] ListingSearchParams p, CancellationToken ct)
        => Ok(await listings.SearchAsync(p, ct));

    [HttpGet("featured")]
    public async Task<ActionResult<IReadOnlyList<ListingDto>>> Featured([FromQuery] int count = 6, CancellationToken ct = default)
        => Ok(await listings.GetFeaturedAsync(count, ct));

    [HttpGet("categories")]
    public ActionResult<IReadOnlyList<string>> Categories() =>
        Ok(new[] { "Cameras", "Power Tools", "Camping Gear", "Sports Equipment", "Event Equipment", "Speakers", "Home Appliances", "Electronics" });

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListingDto>> Get(Guid id, CancellationToken ct)
    {
        var listing = await listings.GetByIdAsync(id, ct);
        return listing is null ? NotFound() : Ok(listing);
    }

    [HttpPost]
    public async Task<ActionResult<ListingDto>> Create([FromBody] CreateListingRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var listing = await listings.CreateAsync(userId.Value, req, ct);
        return CreatedAtAction(nameof(Get), new { id = listing.Id }, listing);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListingDto>> Update(Guid id, [FromBody] CreateListingRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var listing = await listings.UpdateAsync(id, userId.Value, req, ct);
        return listing is null ? NotFound() : Ok(listing);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            return await listings.DeleteAsync(id, userId.Value, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/images")]
    public async Task<ActionResult<ListingImageDto>> UploadImage(Guid id, [FromForm] IFormFile file, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        await using var stream = file.OpenReadStream();
        var image = await listings.AddImageAsync(id, userId.Value, stream, file.FileName, file.ContentType, ct);
        return Ok(image);
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return await listings.DeleteImageAsync(id, imageId, userId.Value, ct) ? NoContent() : NotFound();
    }

    [HttpGet("owner/mine")]
    public async Task<ActionResult<IReadOnlyList<ListingDto>>> MyListings(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await listings.GetByOwnerAsync(userId.Value, ct));
    }

    [HttpGet("owner/dashboard")]
    public async Task<ActionResult<OwnerDashboardDto>> OwnerDashboard(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await listings.GetOwnerDashboardAsync(userId.Value, ct));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

[ApiController]
[Route("api/[controller]")]
public class RentalsController(IRentalService rentals) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RentalDto>> Create([FromBody] CreateRentalRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await rentals.CreateRequestAsync(userId.Value, req, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RentalDto>> Get(Guid id, CancellationToken ct)
    {
        var rental = await rentals.GetByIdAsync(id, ct);
        return rental is null ? NotFound() : Ok(rental);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<RentalDto>> UpdateStatus(Guid id, [FromBody] UpdateRentalStatusRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var rental = await rentals.UpdateStatusAsync(id, userId.Value, req, ct);
            return rental is null ? NotFound() : Ok(rental);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("renter/mine")]
    public async Task<ActionResult<IReadOnlyList<RentalDto>>> MyRentals(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await rentals.GetByRenterAsync(userId.Value, ct));
    }

    [HttpGet("owner/requests")]
    public async Task<ActionResult<IReadOnlyList<RentalDto>>> OwnerRequests(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return Ok(await rentals.GetByOwnerAsync(userId.Value, ct));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

[ApiController]
[Route("api/[controller]")]
public class ReviewsController(
    RentThingsDbContext db,
    ITrustScoreService trust,
    INotificationPublisher notifications) : ControllerBase
{
    [HttpGet("listing/{listingId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetForListing(Guid listingId, CancellationToken ct)
    {
        var reviews = await db.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Rental)
            .Where(r => r.Rental.ListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.RentalId, r.ReviewerId, $"{r.Reviewer.FirstName} {r.Reviewer.LastName}",
                r.Rating, r.Comment, r.IsOwnerReview, r.CreatedAt))
            .ToListAsync(ct);
        return Ok(reviews);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var rental = await db.Rentals.Include(r => r.Listing).Include(r => r.Renter)
            .FirstOrDefaultAsync(r => r.Id == req.RentalId && (r.Status == RentalStatus.Completed || r.Status == RentalStatus.Returned || r.Status == RentalStatus.Reviewed), ct);
        if (rental is null) return BadRequest(new { error = "Rental not found or not completed." });

        var isOwner = rental.Listing.OwnerId == userId;
        var revieweeId = isOwner ? rental.RenterId : rental.Listing.OwnerId;
        if (rental.RenterId != userId && rental.Listing.OwnerId != userId)
            return Forbid();

        var review = new Review
        {
            RentalId = req.RentalId,
            ReviewerId = userId.Value,
            RevieweeId = revieweeId,
            Rating = Math.Clamp(req.Rating, 1, 5),
            Comment = req.Comment,
            IsOwnerReview = isOwner
        };

        db.Reviews.Add(review);

        var reviewNotif = new Notification
        {
            UserId = revieweeId,
            Type = NotificationType.NewReview,
            Title = "New review received",
            Message = $"You received a {review.Rating}-star review.",
            ActionUrl = $"/dashboard/profile"
        };
        db.Notifications.Add(reviewNotif);

        await db.SaveChangesAsync(ct);
        await trust.RecalculateAsync(revieweeId, ct);
        await notifications.PublishAsync(revieweeId, NotificationMapper.ToDto(reviewNotif), ct);

        if (rental.Status == RentalStatus.Returned)
        {
            rental.Status = RentalStatus.Reviewed;
            rental.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await trust.RecalculateAsync(rental.RenterId, ct);
            await trust.RecalculateAsync(rental.Listing.OwnerId, ct);
        }

        await db.Entry(review).Reference(r => r.Reviewer).LoadAsync(ct);
        return Ok(new ReviewDto(review.Id, review.RentalId, review.ReviewerId,
            $"{review.Reviewer.FirstName} {review.Reviewer.LastName}",
            review.Rating, review.Comment, review.IsOwnerReview, review.CreatedAt));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var items = await db.Notifications.Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Type.ToString(), n.Title, n.Message, n.ActionUrl, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (n is null) return NotFound();
        n.IsRead = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        await db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

[ApiController]
[Route("api/[controller]")]
public class AdminController(RentThingsDbContext db, IListingService listings, ITrustScoreService trust) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken ct)
    {
        var totalUsers = await db.Users.CountAsync(ct);
        var totalListings = await db.Listings.CountAsync(l => l.Status == ListingStatus.Active, ct);
        var activeRentals = await db.Rentals.CountAsync(r =>
            r.Status == RentalStatus.Active || r.Status == RentalStatus.Approved || r.Status == RentalStatus.HandedOver, ct);
        var completedRentals = await db.Rentals.CountAsync(r =>
            r.Status == RentalStatus.Completed || r.Status == RentalStatus.Reviewed, ct);
        var revenue = await db.Rentals.Where(r => r.Status == RentalStatus.Completed || r.Status == RentalStatus.Reviewed)
            .SumAsync(r => r.TotalPrice, ct);
        var flagged = await db.Listings.CountAsync(l => l.Status == ListingStatus.Flagged, ct);
        var reports = await db.UserReports.CountAsync(r => !r.IsResolved, ct);

        var monthly = await db.Rentals
            .Where(r => r.Status == RentalStatus.Completed || r.Status == RentalStatus.Reviewed)
            .GroupBy(r => new { r.CompletedAt!.Value.Year, r.CompletedAt!.Value.Month })
            .Select(g => new MonthlyStatDto($"{g.Key.Year}-{g.Key.Month:D2}", g.Count(), g.Sum(r => r.TotalPrice)))
            .OrderBy(m => m.Month)
            .Take(6)
            .ToListAsync(ct);

        var byCategory = await db.Rentals.Include(r => r.Listing)
            .GroupBy(r => r.Listing.Category)
            .Select(g => new CategoryStatDto(g.Key, g.Count()))
            .ToListAsync(ct);

        var byStatus = await db.Rentals
            .GroupBy(r => r.Status)
            .Select(g => new StatusStatDto(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);

        return Ok(new AdminStatsDto(totalUsers, totalListings, activeRentals, completedRentals, revenue, flagged, reports, monthly, byCategory, byStatus));
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers([FromQuery] string? search, [FromQuery] string? role, CancellationToken ct)
    {
        var query = db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var r))
            query = query.Where(u => u.Role == r);

        var users = await query.OrderByDescending(u => u.CreatedAt).Take(100)
            .Select(u => UserMapper.Map(u)).ToListAsync(ct);
        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/trust-score")]
    public async Task<ActionResult<UserDto>> UpdateTrustScore(Guid id, [FromBody] UpdateTrustScoreRequest req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct);
        if (user is null) return NotFound();
        user.TrustScore = Math.Clamp(req.TrustScore, 0, 100);
        user.TrustLevel = trust.GetTrustLevel(user.TrustScore);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(UserMapper.Map(user));
    }

    [HttpPatch("users/{id:guid}/suspend")]
    public async Task<ActionResult<UserDto>> SuspendUser(Guid id, [FromBody] SuspendUserRequest req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct);
        if (user is null) return NotFound();
        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(UserMapper.Map(user));
    }

    [HttpGet("flagged-listings")]
    public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetFlagged(CancellationToken ct)
    {
        var items = await db.Listings.Include(l => l.Owner).Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Flagged || l.Status == ListingStatus.PendingReview)
            .ToListAsync(ct);
        return Ok(items.Select(l => ListingService.MapListing(l)).ToList());
    }

    [HttpPatch("listings/{id:guid}/status")]
    public async Task<IActionResult> UpdateListingStatus(Guid id, [FromBody] UpdateListingStatusRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<ListingStatus>(req.Status, true, out var status))
            return BadRequest(new { error = "Invalid status." });
        return await listings.SetListingStatusAsync(id, status, ct) ? NoContent() : NotFound();
    }

    [HttpGet("rentals")]
    public async Task<ActionResult<IReadOnlyList<RentalDto>>> GetAllRentals(CancellationToken ct)
    {
        var rentals = await db.Rentals.Include(r => r.Listing).ThenInclude(l => l.Images).Include(r => r.Listing).ThenInclude(l => l.Owner)
            .Include(r => r.Renter).OrderByDescending(r => r.CreatedAt).Take(100).ToListAsync(ct);
        return Ok(rentals.Select(r => RentalService.MapRental(r, r.Listing)).ToList());
    }

    [HttpGet("reports")]
    public async Task<ActionResult<IReadOnlyList<ReportDto>>> GetReports(CancellationToken ct)
    {
        var reports = await db.UserReports.OrderByDescending(r => r.CreatedAt).Take(50).ToListAsync(ct);
        var reporterIds = reports.Select(r => r.ReporterId).Distinct().ToList();
        var userIds = reports.Where(r => r.ReportedUserId.HasValue).Select(r => r.ReportedUserId!.Value).Distinct().ToList();
        var listingIds = reports.Where(r => r.ReportedListingId.HasValue).Select(r => r.ReportedListingId!.Value).Distinct().ToList();

        var users = await db.Users.Where(u => reporterIds.Contains(u.Id) || userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, ct);
        var listingsMap = await db.Listings.Where(l => listingIds.Contains(l.Id)).ToDictionaryAsync(l => l.Id, ct);

        var dtos = reports.Select(r => new ReportDto(
            r.Id, r.ReporterId,
            users.TryGetValue(r.ReporterId, out var rep) ? $"{rep.FirstName} {rep.LastName}" : "Unknown",
            r.ReportedUserId,
            r.ReportedUserId.HasValue && users.TryGetValue(r.ReportedUserId.Value, out var ru) ? $"{ru.FirstName} {ru.LastName}" : null,
            r.ReportedListingId,
            r.ReportedListingId.HasValue && listingsMap.TryGetValue(r.ReportedListingId.Value, out var rl) ? rl.Title : null,
            r.Reason, r.Description, r.IsResolved, r.CreatedAt)).ToList();

        return Ok(dtos);
    }

    [HttpPatch("reports/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid id, CancellationToken ct)
    {
        var report = await db.UserReports.FindAsync([id], ct);
        if (report is null) return NotFound();
        report.IsResolved = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class StatsController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet("platform")]
    public async Task<ActionResult<PlatformStatsDto>> Platform(CancellationToken ct)
    {
        var listings = await db.Listings.CountAsync(l => l.Status == ListingStatus.Active, ct);
        var rentals = await db.Rentals.CountAsync(ct);
        var renters = await db.Users.CountAsync(u => u.Role == UserRole.Renter, ct);
        var avgRating = await db.Listings.Where(l => l.ReviewCount > 0).AverageAsync(l => l.AverageRating, ct);
        return Ok(new PlatformStatsDto(listings, rentals, renters, Math.Round(avgRating, 1)));
    }
}

[ApiController]
[Route("api/[controller]")]
public class AiController(IAiServicesClient ai, IAiVisionService vision) : ControllerBase
{
    [HttpPost("generate-listing")]
    public async Task<ActionResult<AiListingSuggestionDto>> GenerateListing(
        [FromForm] IFormFile? image,
        [FromForm] string? hint,
        CancellationToken ct)
    {
        AiListingSuggestion suggestion;
        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            suggestion = await ai.GenerateListingFromImageAsync(stream, hint, ct);
        }
        else
        {
            suggestion = await ai.GenerateListingFromImageAsync(Stream.Null, hint, ct);
        }

        return Ok(new AiListingSuggestionDto(suggestion.Title, suggestion.Description, suggestion.Category,
            suggestion.RentalTips, suggestion.SuggestedCategories));
    }

    [HttpPost("validate-image")]
    public async Task<ActionResult<ImageValidationDto>> ValidateImage([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded. Form field must be named 'file'." });

        await using var stream = file.OpenReadStream();
        var result = await vision.ValidateListingImageAsync(stream, ct);
        return Ok(new ImageValidationDto(result.IsValid, result.HasInappropriateContent, result.IsLowQuality,
            result.IsBlurry, result.HasVisibleObject, result.QualityScore, result.Issues, result.Recommendations,
            result.Category, result.Subcategory, result.Tags ?? [], result.Confidence, result.Flagged));
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponseDto>> Chat([FromBody] AiChatRequest req, CancellationToken ct)
    {
        var response = await ai.ChatAsync(req.Message, req.ConversationId, ct);
        return Ok(new AiChatResponseDto(response.Reply, response.ConversationId));
    }
}

[ApiController]
[Route("api/[controller]")]
public class MapsController(IMapsService maps) : ControllerBase
{
    [HttpGet("geocode")]
    public async Task<ActionResult<GeocodeResultDto>> Geocode([FromQuery] string address, CancellationToken ct)
    {
        var result = await maps.GeocodeAsync(address, ct);
        return result is null ? NotFound() : Ok(new GeocodeResultDto(result.Latitude, result.Longitude, result.FormattedAddress));
    }

    [HttpGet("static")]
    public IActionResult StaticMap([FromQuery] double lat, [FromQuery] double lon, [FromQuery] int zoom = 14)
    {
        var url = maps.GetStaticMapUrl(lat, lon, zoom);
        return Redirect(url);
    }

    [HttpGet("distance")]
    public ActionResult<double> Distance(
        [FromQuery] double lat1, [FromQuery] double lon1,
        [FromQuery] double lat2, [FromQuery] double lon2)
        => Ok(maps.CalculateDistanceKm(lat1, lon1, lat2, lon2));
}

[ApiController]
[Route("api/[controller]")]
public class FavoritesController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListingDto>>> GetFavorites(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var items = await db.Favorites.Include(f => f.Listing).ThenInclude(l => l.Owner)
            .Include(f => f.Listing).ThenInclude(l => l.Images)
            .Where(f => f.UserId == userId).Select(f => f.Listing).ToListAsync(ct);
        return Ok(items.Select(l => ListingService.MapListing(l)).ToList());
    }

    [HttpPost("{listingId:guid}")]
    public async Task<IActionResult> Add(Guid listingId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        if (!await db.Favorites.AnyAsync(f => f.UserId == userId && f.ListingId == listingId, ct))
        {
            db.Favorites.Add(new Favorite { UserId = userId.Value, ListingId = listingId });
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    [HttpDelete("{listingId:guid}")]
    public async Task<IActionResult> Remove(Guid listingId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var fav = await db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId, ct);
        if (fav is not null) { db.Favorites.Remove(fav); await db.SaveChangesAsync(ct); }
        return NoContent();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
