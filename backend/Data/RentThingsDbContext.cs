using Microsoft.EntityFrameworkCore;
using RentThings.Api.Models;

namespace RentThings.Api.Data;

public class RentThingsDbContext(DbContextOptions<RentThingsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<AvailabilityBlock> AvailabilityBlocks => Set<AvailabilityBlock>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TrustScoreHistory> TrustScoreHistories => Set<TrustScoreHistory>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rentthings");

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
            e.Property(u => u.TrustLevel).HasConversion<string>();
        });

        modelBuilder.Entity<Category>(e => e.HasIndex(c => c.Slug).IsUnique());

        modelBuilder.Entity<Listing>(e =>
        {
            e.Property(l => l.Status).HasConversion<string>();
            e.HasOne(l => l.Owner).WithMany(u => u.Listings).HasForeignKey(l => l.OwnerId);
            e.HasOne(l => l.Category).WithMany(c => c.Listings).HasForeignKey(l => l.CategoryId);
        });

        modelBuilder.Entity<Rental>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>();
            e.Property(r => r.PaymentStatus).HasConversion<string>();
            e.HasOne(r => r.Renter).WithMany(u => u.RentalsAsRenter).HasForeignKey(r => r.RenterId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Owner).WithMany(u => u.RentalsAsOwner).HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.Property(r => r.ReviewType).HasConversion<string>();
            e.HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Reviewee).WithMany().HasForeignKey(r => r.RevieweeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e => e.Property(n => n.Type).HasConversion<string>());

        modelBuilder.Entity<Favorite>(e =>
        {
            e.HasKey(f => new { f.UserId, f.ListingId });
            e.HasOne(f => f.User).WithMany(u => u.Favorites).HasForeignKey(f => f.UserId);
            e.HasOne(f => f.Listing).WithMany().HasForeignKey(f => f.ListingId);
        });
    }
}
