using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.DTOs;
using RentThings.Api.Models;
using RentThings.Api.Services;

namespace RentThings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IEntraIdAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await auth.AuthenticateAsync(request, ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me([FromHeader(Name = "Authorization")] string? authHeader, CancellationToken ct)
    {
        var token = authHeader?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token)) return Unauthorized();
        var user = await auth.GetUserFromTokenAsync(token, ct);
        return user is null ? Unauthorized() : Ok(user);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ListingsController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ListingSummaryDto>>> Search([FromQuery] SearchListingsRequest req, CancellationToken ct)
    {
        var query = db.Listings
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Active);

        if (!string.IsNullOrWhiteSpace(req.Query))
            query = query.Where(l => l.Title.Contains(req.Query) || l.Description.Contains(req.Query));
        if (req.CategoryId.HasValue) query = query.Where(l => l.CategoryId == req.CategoryId);
        if (req.MinPrice.HasValue) query = query.Where(l => l.PricePerDay >= req.MinPrice);
        if (req.MaxPrice.HasValue) query = query.Where(l => l.PricePerDay <= req.MaxPrice);
        if (!string.IsNullOrWhiteSpace(req.Location)) query = query.Where(l => l.Location.Contains(req.Location));
        if (req.MinRating.HasValue) query = query.Where(l => l.AverageRating >= req.MinRating);

        query = req.SortBy switch
        {
            "price_asc" => query.OrderBy(l => l.PricePerDay),
            "price_desc" => query.OrderByDescending(l => l.PricePerDay),
            "rating" => query.OrderByDescending(l => l.AverageRating),
            "newest" => query.OrderByDescending(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.IsFeatured).ThenByDescending(l => l.AverageRating)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToListAsync(ct);

        var dtos = items.Select(l => new ListingSummaryDto(
            l.Id, l.Title, l.PricePerDay, l.Location, l.AverageRating, l.ReviewCount, l.IsFeatured,
            l.Images.FirstOrDefault(i => i.IsPrimary)?.BlobUrl ?? l.Images.FirstOrDefault()?.BlobUrl,
            new CategoryDto(l.Category.Id, l.Category.Name, l.Category.Slug, l.Category.Icon, l.Category.Description, l.Category.ItemCount)
        ));

        return Ok(new PagedResult<ListingSummaryDto>(dtos, total, req.Page, req.PageSize));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ListingSummaryDto>>> Featured(CancellationToken ct)
    {
        var items = await db.Listings.Include(l => l.Category).Include(l => l.Images)
            .Where(l => l.IsFeatured && l.Status == ListingStatus.Active).Take(6).ToListAsync(ct);
        return Ok(items.Select(MapSummary));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListingDto>> GetById(Guid id, CancellationToken ct)
    {
        var l = await db.Listings.Include(x => x.Category).Include(x => x.Owner).Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (l is null) return NotFound();
        l.ViewCount++;
        await db.SaveChangesAsync(ct);
        return Ok(MapDetail(l));
    }

    [HttpPost]
    public async Task<ActionResult<ListingDto>> Create([FromBody] CreateListingRequest req, [FromHeader(Name = "X-User-Id")] Guid userId, CancellationToken ct)
    {
        var listing = new Listing
        {
            OwnerId = userId,
            CategoryId = req.CategoryId,
            Title = req.Title,
            Description = req.Description,
            PricePerDay = req.PricePerDay,
            DepositAmount = req.DepositAmount,
            Location = req.Location,
            Status = ListingStatus.PendingReview
        };
        db.Listings.Add(listing);
        await db.SaveChangesAsync(ct);
        await db.Entry(listing).Reference(l => l.Category).LoadAsync(ct);
        await db.Entry(listing).Reference(l => l.Owner).LoadAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, MapDetail(listing));
    }

    [HttpGet("owner/{ownerId:guid}")]
    public async Task<ActionResult<IEnumerable<ListingSummaryDto>>> ByOwner(Guid ownerId, CancellationToken ct)
    {
        var items = await db.Listings.Include(l => l.Category).Include(l => l.Images)
            .Where(l => l.OwnerId == ownerId).ToListAsync(ct);
        return Ok(items.Select(MapSummary));
    }

    private static ListingSummaryDto MapSummary(Listing l) => new(
        l.Id, l.Title, l.PricePerDay, l.Location, l.AverageRating, l.ReviewCount, l.IsFeatured,
        l.Images.FirstOrDefault(i => i.IsPrimary)?.BlobUrl ?? l.Images.FirstOrDefault()?.BlobUrl,
        new CategoryDto(l.Category.Id, l.Category.Name, l.Category.Slug, l.Category.Icon, l.Category.Description, l.Category.ItemCount));

    private static ListingDto MapDetail(Listing l) => new(
        l.Id, l.Title, l.Description, l.PricePerDay, l.DepositAmount, l.Location, l.Status.ToString(),
        l.AverageRating, l.ReviewCount, l.IsFeatured,
        new CategoryDto(l.Category.Id, l.Category.Name, l.Category.Slug, l.Category.Icon, l.Category.Description, l.Category.ItemCount),
        new UserDto(l.Owner.Id, l.Owner.Email, l.Owner.DisplayName, l.Owner.ProfileImageUrl, l.Owner.Role.ToString(), l.Owner.TrustScore, l.Owner.TrustLevel.ToString(), l.Owner.IsVerified, l.Owner.Location),
        l.Images.Select(i => new ListingImageDto(i.Id, i.BlobUrl, i.ThumbnailUrl, i.IsPrimary, i.SortOrder, i.VisionScore, i.VisionIssues)));
}

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var cats = await db.Categories.OrderBy(c => c.Name).ToListAsync(ct);
        return Ok(cats.Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.Icon, c.Description, c.ItemCount)));
    }
}

[ApiController]
[Route("api/[controller]")]
public class RentalsController(RentThingsDbContext db, INotificationService notifications, ICommunicationService comms) : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<RentalDto>>> GetUserRentals(Guid userId, CancellationToken ct)
    {
        var rentals = await db.Rentals.Include(r => r.Listing).ThenInclude(l => l.Images)
            .Include(r => r.Renter).Include(r => r.Owner)
            .Where(r => r.RenterId == userId || r.OwnerId == userId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return Ok(rentals.Select(MapRental));
    }

    [HttpPost]
    public async Task<ActionResult<RentalDto>> Create([FromBody] CreateRentalRequest req, [FromHeader(Name = "X-User-Id")] Guid userId, CancellationToken ct)
    {
        var listing = await db.Listings.FindAsync([req.ListingId], ct);
        if (listing is null) return NotFound("Listing not found");
        if (listing.OwnerId == userId) return BadRequest("Cannot rent your own listing");

        var days = req.EndDate.DayNumber - req.StartDate.DayNumber + 1;
        if (days <= 0) return BadRequest("Invalid date range");

        var rental = new Rental
        {
            ListingId = req.ListingId,
            RenterId = userId,
            OwnerId = listing.OwnerId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalPrice = listing.PricePerDay * days,
            DepositAmount = listing.DepositAmount,
            RenterNotes = req.RenterNotes
        };
        db.Rentals.Add(rental);
        await db.SaveChangesAsync(ct);

        await notifications.CreateNotificationAsync(listing.OwnerId, "New Rental Request",
            $"A renter requested {listing.Title} for {req.StartDate:d} - {req.EndDate:d}", NotificationType.BookingRequest, rental.Id, ct);

        await db.Entry(rental).Reference(r => r.Listing).LoadAsync(ct);
        await db.Entry(rental).Reference(r => r.Renter).LoadAsync(ct);
        await db.Entry(rental).Reference(r => r.Owner).LoadAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, MapRental(rental));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RentalDto>> GetById(Guid id, CancellationToken ct)
    {
        var r = await db.Rentals.Include(x => x.Listing).ThenInclude(l => l.Images).Include(x => x.Renter).Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? NotFound() : Ok(MapRental(r));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<RentalDto>> UpdateStatus(Guid id, [FromBody] UpdateRentalStatusRequest req, CancellationToken ct)
    {
        var rental = await db.Rentals.Include(r => r.Listing).Include(r => r.Renter).Include(r => r.Owner)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rental is null) return NotFound();

        if (Enum.TryParse<RentalStatus>(req.Status, true, out var status))
        {
            rental.Status = status;
            rental.UpdatedAt = DateTime.UtcNow;
            if (status == RentalStatus.Approved) { rental.ApprovedAt = DateTime.UtcNow; rental.PaymentStatus = PaymentStatus.Confirmed; }
            if (status == RentalStatus.Returned) rental.ReturnedAt = DateTime.UtcNow;
            if (status == RentalStatus.Completed) rental.CompletedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return Ok(MapRental(rental));
    }

    private static RentalDto MapRental(Rental r) => new(
        r.Id, r.ListingId, r.Listing.Title,
        r.Listing.Images.FirstOrDefault(i => i.IsPrimary)?.BlobUrl ?? r.Listing.Images.FirstOrDefault()?.BlobUrl,
        r.StartDate, r.EndDate, r.TotalPrice, r.DepositAmount,
        r.Status.ToString(), r.PaymentStatus.ToString(),
        new UserDto(r.Renter.Id, r.Renter.Email, r.Renter.DisplayName, r.Renter.ProfileImageUrl, r.Renter.Role.ToString(), r.Renter.TrustScore, r.Renter.TrustLevel.ToString(), r.Renter.IsVerified, r.Renter.Location),
        new UserDto(r.Owner.Id, r.Owner.Email, r.Owner.DisplayName, r.Owner.ProfileImageUrl, r.Owner.Role.ToString(), r.Owner.TrustScore, r.Owner.TrustLevel.ToString(), r.Owner.IsVerified, r.Owner.Location),
        r.CreatedAt);
}

[ApiController]
[Route("api/[controller]")]
public class ReviewsController(RentThingsDbContext db, ITrustScoreService trustScore) : ControllerBase
{
    [HttpGet("listing/{listingId:guid}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> ByListing(Guid listingId, CancellationToken ct)
    {
        var reviews = await db.Reviews.Include(r => r.Reviewer).Where(r => r.ListingId == listingId && r.IsPublic).ToListAsync(ct);
        return Ok(reviews.Select(r => new ReviewDto(r.Id, r.Rating, r.Comment, r.ReviewType.ToString(),
            new UserDto(r.Reviewer.Id, r.Reviewer.Email, r.Reviewer.DisplayName, r.Reviewer.ProfileImageUrl, r.Reviewer.Role.ToString(), r.Reviewer.TrustScore, r.Reviewer.TrustLevel.ToString(), r.Reviewer.IsVerified, r.Reviewer.Location), r.CreatedAt)));
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest req, [FromHeader(Name = "X-User-Id")] Guid userId, CancellationToken ct)
    {
        var rental = await db.Rentals.Include(r => r.Listing).FirstOrDefaultAsync(r => r.Id == req.RentalId, ct);
        if (rental is null) return NotFound();
        var revieweeId = rental.RenterId == userId ? rental.OwnerId : rental.RenterId;

        var review = new Review
        {
            RentalId = req.RentalId, ReviewerId = userId, RevieweeId = revieweeId,
            ListingId = rental.ListingId, Rating = req.Rating, Comment = req.Comment,
            ReviewType = Enum.Parse<ReviewType>(req.ReviewType, true)
        };
        db.Reviews.Add(review);

        var listingReviews = await db.Reviews.Where(r => r.ListingId == rental.ListingId).ToListAsync(ct);
        listingReviews.Add(review);
        rental.Listing.AverageRating = (decimal)listingReviews.Average(r => r.Rating);
        rental.Listing.ReviewCount = listingReviews.Count;

        await db.SaveChangesAsync(ct);
        await trustScore.RecalculateScoreAsync(revieweeId, ct);

        await db.Entry(review).Reference(r => r.Reviewer).LoadAsync(ct);
        return Ok(new ReviewDto(review.Id, review.Rating, review.Comment, review.ReviewType.ToString(),
            new UserDto(review.Reviewer.Id, review.Reviewer.Email, review.Reviewer.DisplayName, review.Reviewer.ProfileImageUrl, review.Reviewer.Role.ToString(), review.Reviewer.TrustScore, review.Reviewer.TrustLevel.ToString(), review.Reviewer.IsVerified, review.Reviewer.Location), review.CreatedAt));
    }
}

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAll(Guid userId, CancellationToken ct)
        => Ok(await notifications.GetUserNotificationsAsync(userId, ct));

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, [FromHeader(Name = "X-User-Id")] Guid userId, CancellationToken ct)
    {
        await notifications.MarkAsReadAsync(id, userId, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController(RentThingsDbContext db, ITrustScoreService trustScore) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(Guid id, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([id], ct);
        return u is null ? NotFound() : Ok(new UserProfileDto(u.Id, u.Email, u.DisplayName, u.PhoneNumber, u.ProfileImageUrl, u.Role.ToString(), u.TrustScore, u.TrustLevel.ToString(), u.IsVerified, u.Location, u.Bio, u.CreatedAt));
    }

    [HttpGet("{id:guid}/trust-score")]
    public async Task<ActionResult<TrustScoreDto>> GetTrustScore(Guid id, CancellationToken ct)
    {
        var u = await db.Users.FindAsync([id], ct);
        return u is null ? NotFound() : Ok(trustScore.GetTrustScore(u));
    }
}

[ApiController]
[Route("api/[controller]")]
public class AdminController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet("analytics")]
    public async Task<ActionResult<AdminAnalyticsDto>> Analytics(CancellationToken ct)
    {
        var totalUsers = await db.Users.CountAsync(ct);
        var totalListings = await db.Listings.CountAsync(ct);
        var totalRentals = await db.Rentals.CountAsync(ct);
        var activeRentals = await db.Rentals.CountAsync(r => r.Status == RentalStatus.Active, ct);
        var revenue = await db.Rentals.Where(r => r.PaymentStatus == PaymentStatus.Confirmed).SumAsync(r => r.TotalPrice, ct);

        return Ok(new AdminAnalyticsDto(totalUsers, totalListings, totalRentals, activeRentals, revenue,
            [new("Jan", 12), new("Feb", 19), new("Mar", 24), new("Apr", 31), new("May", 28), new("Jun", 35)],
            [new("Jan", 45), new("Feb", 52), new("Mar", 61), new("Apr", 78), new("May", 89), new("Jun", 102)]));
    }

    [HttpGet("reports")]
    public async Task<ActionResult<IEnumerable<object>>> Reports(CancellationToken ct)
    {
        var reports = await db.Reports.Include(r => r.Reporter).OrderByDescending(r => r.CreatedAt).Take(20).ToListAsync(ct);
        return Ok(reports.Select(r => new { r.Id, r.TargetType, r.TargetId, r.Reason, r.Status, Reporter = r.Reporter.DisplayName, r.CreatedAt }));
    }

    [HttpGet("flagged-listings")]
    public async Task<ActionResult<IEnumerable<ListingSummaryDto>>> Flagged(CancellationToken ct)
    {
        var items = await db.Listings.Include(l => l.Category).Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Flagged || l.Status == ListingStatus.PendingReview).ToListAsync(ct);
        return Ok(items.Select(l => new ListingSummaryDto(l.Id, l.Title, l.PricePerDay, l.Location, l.AverageRating, l.ReviewCount, l.IsFeatured,
            l.Images.FirstOrDefault()?.BlobUrl, new CategoryDto(l.Category.Id, l.Category.Name, l.Category.Slug, l.Category.Icon, l.Category.Description, l.Category.ItemCount))));
    }
}

[ApiController]
[Route("api/[controller]")]
public class AiController(IAiListingService listingAi, IAiVisionService vision, IAiChatService chat, IBlobStorageService blob) : ControllerBase
{
    [HttpPost("generate-listing")]
    public async Task<ActionResult<AiListingGenerationResponse>> GenerateListing([FromBody] AiListingGenerationRequest req, CancellationToken ct)
        => Ok(await listingAi.GenerateListingFromImageAsync(req, ct));

    [HttpPost("validate-image")]
    public async Task<ActionResult<VisionValidationResult>> ValidateImage(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await vision.ValidateImageAsync(stream, ct));
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponse>> Chat([FromBody] AiChatRequest req, CancellationToken ct)
        => Ok(await chat.ChatAsync(req, ct));

    [HttpPost("upload-image")]
    public async Task<ActionResult<object>> UploadImage(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var url = await blob.UploadListingImageAsync(stream, file.FileName, file.ContentType, ct);
        var validation = await vision.ValidateImageAsync(file.OpenReadStream(), ct);
        return Ok(new { url, thumbnailUrl = url.Replace("/listings/", "/thumbnails/"), validation });
    }
}

[ApiController]
[Route("api/[controller]")]
public class StatsController(RentThingsDbContext db) : ControllerBase
{
    [HttpGet("platform")]
    public async Task<ActionResult<PlatformStatsDto>> Platform(CancellationToken ct)
    {
        var users = await db.Users.CountAsync(ct);
        var listings = await db.Listings.CountAsync(l => l.Status == ListingStatus.Active, ct);
        var completed = await db.Rentals.CountAsync(r => r.Status == RentalStatus.Completed, ct);
        var avgRating = await db.Listings.Where(l => l.ReviewCount > 0).AverageAsync(l => (decimal?)l.AverageRating, ct) ?? 0;
        return Ok(new PlatformStatsDto(users, listings, completed, avgRating));
    }
}
