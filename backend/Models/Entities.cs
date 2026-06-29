namespace RentThings.Api.Models;

public enum UserRole { Renter, Owner, Admin }
public enum TrustLevel { Bronze, Silver, Gold, Platinum }
public enum ListingStatus { Draft, PendingReview, Active, Inactive, Flagged }
public enum RentalStatus { Pending, Approved, Active, Returned, Completed, Cancelled, Rejected }
public enum PaymentStatus { Pending, Confirmed, Refunded }
public enum NotificationType { BookingRequest, Approval, ReturnReminder, Review, System }
public enum ReviewType { RenterToOwner, OwnerToRenter, ListingReview }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? EntraObjectId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Renter;
    public bool IsVerified { get; set; }
    public int TrustScore { get; set; } = 50;
    public TrustLevel TrustLevel { get; set; } = TrustLevel.Bronze;
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Listing> Listings { get; set; } = [];
    public ICollection<Rental> RentalsAsRenter { get; set; } = [];
    public ICollection<Rental> RentalsAsOwner { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
}

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public int ItemCount { get; set; }
    public ICollection<Listing> Listings { get; set; } = [];
}

public class Listing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal DepositAmount { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool AiGeneratedDescription { get; set; }
    public bool VisionValidationPassed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public User Owner { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<ListingImage> Images { get; set; } = [];
    public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; } = [];
    public ICollection<Rental> Rentals { get; set; } = [];
}

public class ListingImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public decimal? VisionScore { get; set; }
    public string? VisionIssues { get; set; }
    public Listing Listing { get; set; } = null!;
}

public class AvailabilityBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }
    public Listing Listing { get; set; } = null!;
}

public class Rental
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public Guid RenterId { get; set; }
    public Guid OwnerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DepositAmount { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? OwnerNotes { get; set; }
    public string? RenterNotes { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Listing Listing { get; set; } = null!;
    public User Renter { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<Review> Reviews { get; set; } = [];
}

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RentalId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public Guid ListingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public ReviewType ReviewType { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Rental Rental { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
    public User Reviewee { get; set; } = null!;
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}

public class TrustScoreHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int PreviousScore { get; set; }
    public int NewScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}

public class Favorite
{
    public Guid UserId { get; set; }
    public Guid ListingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReporterId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User Reporter { get; set; } = null!;
}
