namespace RentThings.Api.Models;

public class Rental
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public Guid RenterId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Requested;
    public decimal TotalPrice { get; set; }
    public decimal DepositAmount { get; set; }
    public string? Message { get; set; }
    public string? OwnerNotes { get; set; }
    public bool IsLateReturn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Listing Listing { get; set; } = null!;
    public User Renter { get; set; } = null!;
    public ICollection<Review> Reviews { get; set; } = [];
}
