namespace RentThings.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public string? EntraObjectId { get; set; }
    public UserRole Role { get; set; } = UserRole.Renter;
    public int TrustScore { get; set; } = 50;
    public TrustLevel TrustLevel { get; set; } = TrustLevel.Silver;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Listing> Listings { get; set; } = [];
    public ICollection<Rental> RentalsAsRenter { get; set; } = [];
    public ICollection<Review> ReviewsGiven { get; set; } = [];
    public ICollection<Review> ReviewsReceived { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
}
