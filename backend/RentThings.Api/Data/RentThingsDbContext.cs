using Microsoft.EntityFrameworkCore;
using RentThings.Api.Models;

namespace RentThings.Api.Data;

public class RentThingsDbContext(DbContextOptions<RentThingsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<ListingAvailability> ListingAvailability => Set<ListingAvailability>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<UserReport> UserReports => Set<UserReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.TrustScore).HasDefaultValue(50);
        });

        modelBuilder.Entity<Listing>(e =>
        {
            e.Property(l => l.PricePerDay).HasPrecision(18, 2);
            e.Property(l => l.Deposit).HasPrecision(18, 2);
            e.HasOne(l => l.Owner).WithMany(u => u.Listings).HasForeignKey(l => l.OwnerId);
        });

        modelBuilder.Entity<Rental>(e =>
        {
            e.Property(r => r.TotalPrice).HasPrecision(18, 2);
            e.Property(r => r.DepositAmount).HasPrecision(18, 2);
            e.HasOne(r => r.Listing).WithMany(l => l.Rentals).HasForeignKey(r => r.ListingId);
            e.HasOne(r => r.Renter).WithMany(u => u.RentalsAsRenter).HasForeignKey(r => r.RenterId);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasOne(r => r.Reviewer).WithMany(u => u.ReviewsGiven).HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Reviewee).WithMany(u => u.ReviewsReceived).HasForeignKey(r => r.RevieweeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Favorite>(e =>
        {
            e.HasIndex(f => new { f.UserId, f.ListingId }).IsUnique();
        });

        modelBuilder.Entity<Favorite>()
        .HasOne(f => f.User)
        .WithMany()
        .HasForeignKey(f => f.UserId)
        .OnDelete(DeleteBehavior.NoAction); // Cascade වෙනුවට NoAction දමන්න

    // Rentals චක්‍රීය Cascade Delete එක වැළැක්වීම (මෙහිද ප්‍රශ්න මතු විය හැක)
    modelBuilder.Entity<Rental>()
        .HasOne(r => r.Renter)
        .WithMany()
        .HasForeignKey(r => r.RenterId)
        .OnDelete(DeleteBehavior.NoAction);

    // Reviews චක්‍රීය Cascade Delete එක වැළැක්වීම
    modelBuilder.Entity<Review>()
        .HasOne(r => r.Reviewer)
        .WithMany()
        .HasForeignKey(r => r.ReviewerId)
        .OnDelete(DeleteBehavior.NoAction);
    }
}
