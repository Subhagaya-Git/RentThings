namespace RentThings.Api.Models;

public class Listing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<ListingImage> Images { get; set; } = [];
    public ICollection<Rental> Rentals { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<ListingAvailability> Availability { get; set; } = [];
}

public class ListingImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public bool PassedValidation { get; set; } = true;
    public string? ValidationNotes { get; set; }

    public Listing Listing { get; set; } = null!;
}

public class ListingAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Listing Listing { get; set; } = null!;
}
