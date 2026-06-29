using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.Models;

namespace RentThings.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RentThingsDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var admin = new User { Email = "admin@rentthings.com", DisplayName = "Platform Admin", Role = UserRole.Admin, IsVerified = true, TrustScore = 95, TrustLevel = TrustLevel.Platinum, Location = "Seattle, WA" };
        var owner1 = new User { Email = "owner@rentthings.com", DisplayName = "Sarah Chen", Role = UserRole.Owner, IsVerified = true, TrustScore = 82, TrustLevel = TrustLevel.Gold, Location = "San Francisco, CA", Bio = "Photography enthusiast renting professional gear." };
        var owner2 = new User { Email = "mike@rentthings.com", DisplayName = "Mike Torres", Role = UserRole.Owner, IsVerified = true, TrustScore = 76, TrustLevel = TrustLevel.Gold, Location = "Austin, TX", Bio = "Power tools and outdoor equipment." };
        var renter = new User { Email = "renter@rentthings.com", DisplayName = "Alex Johnson", Role = UserRole.Renter, IsVerified = true, TrustScore = 68, TrustLevel = TrustLevel.Silver, Location = "Portland, OR" };

        db.Users.AddRange(admin, owner1, owner2, renter);

        var categories = new[]
        {
            new Category { Name = "Cameras & Photography", Slug = "cameras", Icon = "camera", Description = "DSLRs, lenses, lighting" },
            new Category { Name = "Power Tools", Slug = "tools", Icon = "wrench", Description = "Drills, saws, sanders" },
            new Category { Name = "Camping & Outdoor", Slug = "camping", Icon = "tent", Description = "Tents, sleeping bags, stoves" },
            new Category { Name = "Sports Equipment", Slug = "sports", Icon = "dumbbell", Description = "Bikes, skis, fitness gear" },
            new Category { Name = "Event Equipment", Slug = "events", Icon = "party-popper", Description = "Projectors, speakers, tents" },
            new Category { Name = "Home Appliances", Slug = "appliances", Icon = "home", Description = "Vacuums, pressure washers" },
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var camCat = categories[0];
        var toolCat = categories[1];
        var campCat = categories[2];
        var eventCat = categories[4];

        var listings = new List<Listing>
        {
            new() { OwnerId = owner1.Id, CategoryId = camCat.Id, Title = "Canon EOS R5 Full Frame Mirrorless", Description = "Professional 45MP mirrorless camera with RF 24-70mm f/2.8 lens. Includes 2 batteries, charger, and padded case.", PricePerDay = 85, DepositAmount = 500, Location = "San Francisco, CA", Status = ListingStatus.Active, AverageRating = 4.9m, ReviewCount = 24, IsFeatured = true, VisionValidationPassed = true },
            new() { OwnerId = owner1.Id, CategoryId = camCat.Id, Title = "Sony A7 IV with G Master Lens", Description = "33MP hybrid shooter perfect for photo and video. Includes 35mm f/1.4 GM lens.", PricePerDay = 75, DepositAmount = 450, Location = "San Francisco, CA", Status = ListingStatus.Active, AverageRating = 4.8m, ReviewCount = 18, IsFeatured = true, VisionValidationPassed = true },
            new() { OwnerId = owner2.Id, CategoryId = toolCat.Id, Title = "DeWalt 20V Max Combo Kit", Description = "Drill/driver, impact driver, 2 batteries, charger, and tool bag.", PricePerDay = 25, DepositAmount = 100, Location = "Austin, TX", Status = ListingStatus.Active, AverageRating = 4.7m, ReviewCount = 31, VisionValidationPassed = true },
            new() { OwnerId = owner2.Id, CategoryId = toolCat.Id, Title = "Makita Circular Saw + Table", Description = "7-1/4\" circular saw with folding work table. Great for DIY projects.", PricePerDay = 20, DepositAmount = 75, Location = "Austin, TX", Status = ListingStatus.Active, AverageRating = 4.6m, ReviewCount = 12, VisionValidationPassed = true },
            new() { OwnerId = owner2.Id, CategoryId = campCat.Id, Title = "4-Person Camping Tent Set", Description = "Waterproof tent, 4 sleeping bags, camp stove, and lantern. Everything for a weekend trip.", PricePerDay = 35, DepositAmount = 150, Location = "Austin, TX", Status = ListingStatus.Active, AverageRating = 4.5m, ReviewCount = 9, IsFeatured = true, VisionValidationPassed = true },
            new() { OwnerId = owner1.Id, CategoryId = eventCat.Id, Title = "Epson 4K Projector + Screen", Description = "3500 lumen 4K projector with 100\" portable screen. Perfect for outdoor movie nights.", PricePerDay = 60, DepositAmount = 300, Location = "San Francisco, CA", Status = ListingStatus.Active, AverageRating = 4.9m, ReviewCount = 15, IsFeatured = true, VisionValidationPassed = true },
            new() { OwnerId = owner1.Id, CategoryId = eventCat.Id, Title = "JBL PartyBox 310 Speaker", Description = "240W portable Bluetooth speaker with light show. Up to 18 hours battery.", PricePerDay = 40, DepositAmount = 200, Location = "San Francisco, CA", Status = ListingStatus.Active, AverageRating = 4.8m, ReviewCount = 22, VisionValidationPassed = true },
        };
        db.Listings.AddRange(listings);
        await db.SaveChangesAsync();

        var images = new[] { "camera", "camera2", "tools", "tools2", "camping", "projector", "speaker" };
        for (var i = 0; i < listings.Count; i++)
        {
            db.ListingImages.Add(new ListingImage
            {
                ListingId = listings[i].Id,
                BlobUrl = $"https://images.unsplash.com/photo-{1500000000000 + i * 111}?w=800",
                ThumbnailUrl = $"https://images.unsplash.com/photo-{1500000000000 + i * 111}?w=400",
                IsPrimary = true,
                SortOrder = 0,
                VisionScore = 85 + i
            });
        }

        foreach (var c in categories)
            c.ItemCount = listings.Count(l => l.CategoryId == c.Id);

        db.Notifications.AddRange(
            new Notification { UserId = owner1.Id, Title = "New Rental Request", Message = "Alex Johnson requested your Canon EOS R5.", Type = NotificationType.BookingRequest },
            new Notification { UserId = renter.Id, Title = "Rental Approved", Message = "Your camping tent rental has been approved!", Type = NotificationType.Approval, IsRead = true }
        );

        await db.SaveChangesAsync();
    }
}
