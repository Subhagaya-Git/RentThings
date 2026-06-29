using Microsoft.EntityFrameworkCore;
using RentThings.Api.Data;
using RentThings.Api.Models;

namespace RentThings.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RentThingsDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "admin@rentthings.com",
            FirstName = "Alex",
            LastName = "Admin",
            Role = UserRole.Admin,
            TrustScore = 100,
            TrustLevel = TrustLevel.Platinum,
            IsVerified = true,
            Location = "Colombo"
        };

        var owner = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "owner@rentthings.com",
            FirstName = "Olivia",
            LastName = "Owner",
            Role = UserRole.Owner,
            TrustScore = 82,
            TrustLevel = TrustLevel.Platinum,
            IsVerified = true,
            Location = "Kandy"
        };

        var renter = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Email = "renter@rentthings.com",
            FirstName = "Ryan",
            LastName = "Renter",
            Role = UserRole.Renter,
            TrustScore = 68,
            TrustLevel = TrustLevel.Gold,
            IsVerified = true,
            Location = "Colombo",
            Phone = "+94771234567"
        };

        var owner2 = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Email = "owner2@rentthings.com",
            FirstName = "Nimal",
            LastName = "Perera",
            Role = UserRole.Owner,
            TrustScore = 45,
            TrustLevel = TrustLevel.Silver,
            IsVerified = true,
            Location = "Galle"
        };

        context.Users.AddRange(admin, owner, renter, owner2);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var availEnd = today.AddDays(90);

        var listings = new List<Listing>
        {
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                OwnerId = owner.Id,
                Title = "Sony A7 IV Mirrorless Camera Kit",
                Description = "Professional full-frame camera with 28-70mm lens, extra battery, and carrying case. Perfect for events and photography projects.",
                Category = "Cameras",
                PricePerDay = 8500m,
                Deposit = 50000m,
                Location = "Kandy, Central Province",
                City = "Kandy",
                State = "Central",
                Latitude = 7.2906,
                Longitude = 80.6337,
                Status = ListingStatus.Active,
                AverageRating = 4.9,
                ReviewCount = 47,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600&q=80", IsPrimary = true, SortOrder = 0 },
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1606983340126-99ab4feaa64a?w=600&q=80", SortOrder = 1 }
                ]
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                OwnerId = owner.Id,
                Title = "DeWalt 20V Power Tool Combo Set",
                Description = "Complete drill/driver kit with impact driver, two batteries, charger, and tool bag. Ideal for home projects.",
                Category = "Power Tools",
                PricePerDay = 3500m,
                Deposit = 15000m,
                Location = "Kandy, Central Province",
                City = "Kandy",
                State = "Central",
                Latitude = 7.2906,
                Longitude = 80.6337,
                Status = ListingStatus.Active,
                AverageRating = 4.7,
                ReviewCount = 23,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                OwnerId = owner.Id,
                Title = "4-Person Camping Tent & Gear Bundle",
                Description = "Waterproof tent, sleeping bags, camp stove, and lantern. Everything you need for a weekend adventure.",
                Category = "Camping Gear",
                PricePerDay = 4500m,
                Deposit = 20000m,
                Location = "Nuwara Eliya, Central Province",
                City = "Nuwara Eliya",
                State = "Central",
                Latitude = 6.9497,
                Longitude = 80.7891,
                Status = ListingStatus.Active,
                AverageRating = 4.8,
                ReviewCount = 31,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                OwnerId = owner2.Id,
                Title = "Epson 4K Projector + 120\" Screen",
                Description = "Bright 4K projector with portable screen and HDMI cables. Great for movie nights and presentations.",
                Category = "Event Equipment",
                PricePerDay = 6500m,
                Deposit = 35000m,
                Location = "Colombo, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.9271,
                Longitude = 79.8612,
                Status = ListingStatus.Active,
                AverageRating = 4.6,
                ReviewCount = 18,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                OwnerId = owner2.Id,
                Title = "JBL PartyBox 310 Bluetooth Speaker",
                Description = "Powerful portable speaker with light show. Perfect for parties, weddings, and outdoor events.",
                Category = "Speakers",
                PricePerDay = 4000m,
                Deposit = 18000m,
                Location = "Galle, Southern Province",
                City = "Galle",
                State = "Southern",
                Latitude = 6.0535,
                Longitude = 80.2210,
                Status = ListingStatus.Active,
                AverageRating = 4.5,
                ReviewCount = 12,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1545454675-3531b543be5d?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                OwnerId = owner.Id,
                Title = "Dyson V15 Detect Vacuum",
                Description = "Premium cordless vacuum with laser dust detection. Deep clean carpets and hard floors effortlessly.",
                Category = "Home Appliances",
                PricePerDay = 2500m,
                Deposit = 12000m,
                Location = "Colombo, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.9271,
                Longitude = 79.8612,
                Status = ListingStatus.Inactive,
                AverageRating = 4.9,
                ReviewCount = 56,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                OwnerId = owner2.Id,
                Title = "DJI Mini 3 Pro Drone",
                Description = "Compact drone with 4K camera. Pending admin review.",
                Category = "Electronics",
                PricePerDay = 5500m,
                Deposit = 40000m,
                Location = "Colombo, Western Province",
                City = "Colombo",
                State = "Western",
                Status = ListingStatus.PendingReview,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1473968512647-3e447244af8f?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                OwnerId = owner.Id,
                Title = "Suspicious Listing - Flagged Item",
                Description = "This listing was flagged for review.",
                Category = "Electronics",
                PricePerDay = 100m,
                Deposit = 50m,
                Location = "Colombo",
                Status = ListingStatus.Flagged,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1498049794561-7780f7231661?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10111111-1111-1111-1111-111111111111"),
                OwnerId = owner2.Id,
                Title = "Bajaj RE Three-Wheeler (Tuk-Tuk) — Colombo City Tours",
                Description = "Well-maintained tuk-tuk with valid revenue license. Ideal for tourists exploring Pettah, Galle Face, and Colombo Fort. Fuel-efficient four-stroke engine, clean interior, and friendly handover briefing included.",
                Category = "Sports Equipment",
                PricePerDay = 9500m,
                Deposit = 75000m,
                Location = "Colombo 03, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.9271,
                Longitude = 79.8612,
                Status = ListingStatus.Active,
                AverageRating = 4.6,
                ReviewCount = 34,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10222222-2222-2222-2222-222222222222"),
                OwnerId = owner.Id,
                Title = "Honda Dio Scooter — Negombo Beach Runs",
                Description = "125cc automatic scooter, perfect for coastal rides from Negombo to Waikkal. Two helmets, phone mount, and rain cover included. Valid insurance and service records available on request.",
                Category = "Sports Equipment",
                PricePerDay = 2800m,
                Deposit = 25000m,
                Location = "Negombo, Western Province",
                City = "Negombo",
                State = "Western",
                Latitude = 7.2084,
                Longitude = 79.8358,
                Status = ListingStatus.Active,
                AverageRating = 4.8,
                ReviewCount = 41,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10333333-3333-3333-3333-333333333333"),
                OwnerId = owner2.Id,
                Title = "Surfboard & Wetsuit Bundle — Arugam Bay Season",
                Description = "7ft soft-top surfboard with leash, wax, and 3mm wetsuit. Great for beginners and intermediate surfers hitting Main Point or Whiskey Point. Pickup near Arugam Bay main road.",
                Category = "Sports Equipment",
                PricePerDay = 3200m,
                Deposit = 18000m,
                Location = "Arugam Bay, Eastern Province",
                City = "Matara",
                State = "Eastern",
                Latitude = 6.8404,
                Longitude = 81.8361,
                Status = ListingStatus.Active,
                AverageRating = 4.7,
                ReviewCount = 19,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1502680390469-be75c86b576f?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10444444-4444-4444-4444-444444444444"),
                OwnerId = owner2.Id,
                Title = "Wedding Marquee, Chairs & Tables — 150 Guest Setup",
                Description = "Complete outdoor wedding package: white marquee tent, 150 plastic chairs, 15 round tables, and basic lighting. Popular for home garden weddings in Colombo and suburbs. Delivery and setup available for extra fee.",
                Category = "Event Equipment",
                PricePerDay = 22000m,
                Deposit = 100000m,
                Location = "Colombo 07, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.9147,
                Longitude = 79.8630,
                Status = ListingStatus.Active,
                AverageRating = 4.9,
                ReviewCount = 28,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1519225421980-715cb0215aed?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10555555-5555-5555-5555-555555555555"),
                OwnerId = owner.Id,
                Title = "Canon 5D Mark IV Wedding Photography Kit",
                Description = "Full wedding shoot kit: Canon 5D Mark IV body, 24-70mm f/2.8 and 70-200mm f/2.8 lenses, speedlite flash, spare batteries, and padded hard case. Trusted by Kandy and hill-country wedding photographers.",
                Category = "Cameras",
                PricePerDay = 12000m,
                Deposit = 85000m,
                Location = "Kandy, Central Province",
                City = "Kandy",
                State = "Central",
                Latitude = 7.2906,
                Longitude = 80.6337,
                Status = ListingStatus.Active,
                AverageRating = 5.0,
                ReviewCount = 52,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1606800053562-af1a6d610a19?w=600&q=80", IsPrimary = true, SortOrder = 0 },
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1493863641943-9b6717f79915?w=600&q=80", SortOrder = 1 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10666666-6666-6666-6666-666666666666"),
                OwnerId = owner.Id,
                Title = "Ella Trekking Pack — Hammock, Poles & Rain Cover",
                Description = "Lightweight gear for Little Adam's Peak and Ella Rock hikes: parachute hammock with tree straps, carbon trekking poles, waterproof backpack cover, and headlamp. Pickup from Ella town centre.",
                Category = "Camping Gear",
                PricePerDay = 1800m,
                Deposit = 8000m,
                Location = "Ella, Uva Province",
                City = "Ella",
                State = "Uva",
                Latitude = 6.8667,
                Longitude = 81.0466,
                Status = ListingStatus.Active,
                AverageRating = 4.8,
                ReviewCount = 37,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1478131143081-80f7f84ca84d?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10777777-7777-7777-7777-777777777777"),
                OwnerId = owner.Id,
                Title = "Karcher K5 Pressure Washer — Deep Clean Driveways",
                Description = "High-pressure washer for tile roofs, driveways, and vehicle cleaning. 10m hose, patio cleaner attachment, and detergent bottle included. Ideal before avurudu or wedding prep at home.",
                Category = "Home Appliances",
                PricePerDay = 3500m,
                Deposit = 15000m,
                Location = "Matara, Southern Province",
                City = "Matara",
                State = "Southern",
                Latitude = 5.9483,
                Longitude = 80.5353,
                Status = ListingStatus.Active,
                AverageRating = 4.5,
                ReviewCount = 14,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10888888-8888-8888-8888-888888888889"),
                OwnerId = owner2.Id,
                Title = "Yamaha DXR12 PA System — Tamil & Sinhala Events",
                Description = "Professional PA with two powered speakers, subwoofer, mixer, and wireless mics. Perfect for Jaffna cultural shows, church events, and community gatherings. Operator optional.",
                Category = "Speakers",
                PricePerDay = 8500m,
                Deposit = 45000m,
                Location = "Jaffna, Northern Province",
                City = "Jaffna",
                State = "Northern",
                Latitude = 9.6615,
                Longitude = 80.0255,
                Status = ListingStatus.Active,
                AverageRating = 4.7,
                ReviewCount = 22,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1511379938545-c8f1981980a2?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("10999999-9999-9999-9999-999999999999"),
                OwnerId = owner.Id,
                Title = "DJI Mavic 3 Pro — Hill Country Aerial Footage",
                Description = "Fly over tea estates and waterfalls with this 4K HDR drone. Includes three batteries, ND filters, carrying case, and basic flight training on handover. Registered with CAASL guidelines in mind.",
                Category = "Drones",
                PricePerDay = 7500m,
                Deposit = 60000m,
                Location = "Nuwara Eliya, Central Province",
                City = "Nuwara Eliya",
                State = "Central",
                Latitude = 6.9497,
                Longitude = 80.7891,
                Status = ListingStatus.Active,
                AverageRating = 4.9,
                ReviewCount = 16,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1473968512647-3e447244af8f?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000001"),
                OwnerId = owner2.Id,
                Title = "Bosch Professional Drill & Impact Driver Set — Galle Renovations",
                Description = "Cordless drill/driver combo with masonry bits, hole saws, and spirit level. Handy for home renovations in the Galle Fort area or furniture assembly before a homestay opening.",
                Category = "Power Tools",
                PricePerDay = 2800m,
                Deposit = 12000m,
                Location = "Galle Fort, Southern Province",
                City = "Galle",
                State = "Southern",
                Latitude = 6.0269,
                Longitude = 80.2170,
                Status = ListingStatus.Active,
                AverageRating = 4.6,
                ReviewCount = 11,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000002"),
                OwnerId = owner.Id,
                Title = "Trek Marlin 7 Mountain Bike — Ella Rail Trail",
                Description = "Hardtail MTB suited for the Demodara–Ella scenic route and tea estate trails. Helmet, repair kit, and bike lock included. Frame size M/L available — confirm before booking.",
                Category = "Sports Equipment",
                PricePerDay = 2200m,
                Deposit = 20000m,
                Location = "Ella, Uva Province",
                City = "Ella",
                State = "Uva",
                Latitude = 6.8730,
                Longitude = 81.0560,
                Status = ListingStatus.Active,
                AverageRating = 4.8,
                ReviewCount = 29,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1571068316344-75bc76f77890?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000003"),
                OwnerId = owner.Id,
                Title = "40ft Party Tent with Side Walls — Negombo Functions",
                Description = "Heavy-duty PVC party tent with side walls, pegs, and ropes. Fits birthdays, almsgivings, and corporate events near Negombo lagoon. Ground mats available on request.",
                Category = "Event Equipment",
                PricePerDay = 12000m,
                Deposit = 50000m,
                Location = "Negombo, Western Province",
                City = "Negombo",
                State = "Western",
                Latitude = 7.2008,
                Longitude = 79.8737,
                Status = ListingStatus.Active,
                AverageRating = 4.4,
                ReviewCount = 9,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000004"),
                OwnerId = owner2.Id,
                Title = "GoPro Hero 12 + Dome Port — Mirissa Whale Watching",
                Description = "Action camera kit for ocean adventures: Hero 12, waterproof housing, dome port for half-underwater shots, chest mount, and floaty handle. Capture blue whales and surfers alike.",
                Category = "Cameras",
                PricePerDay = 4500m,
                Deposit = 30000m,
                Location = "Mirissa, Southern Province",
                City = "Matara",
                State = "Southern",
                Latitude = 5.9483,
                Longitude = 80.4585,
                Status = ListingStatus.Active,
                AverageRating = 4.7,
                ReviewCount = 24,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000005"),
                OwnerId = owner.Id,
                Title = "5kVA Portable Generator — Outdoor Events & Outages",
                Description = "Petrol generator with voltage stabilizer and extension cords. Keeps sound systems and lights running during garden weddings or power cuts in Anuradhapura district. Fuel not included.",
                Category = "Power Tools",
                PricePerDay = 5500m,
                Deposit = 35000m,
                Location = "Anuradhapura, North Central Province",
                City = "Anuradhapura",
                State = "North Central",
                Latitude = 8.3114,
                Longitude = 80.4037,
                Status = ListingStatus.Active,
                AverageRating = 4.3,
                ReviewCount = 8,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1621905251189-08d45b6b8032?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000006"),
                OwnerId = owner.Id,
                Title = "Kandyan Wedding Poruwa & Oil Lamp Set",
                Description = "Traditional wooden poruwa platform, decorated oil lamps ( pola ), and basic aisle markers for authentic Kandyan ceremonies. Delivery within Kandy MC limits included.",
                Category = "Event Equipment",
                PricePerDay = 15000m,
                Deposit = 80000m,
                Location = "Kandy, Central Province",
                City = "Kandy",
                State = "Central",
                Latitude = 7.2964,
                Longitude = 80.6350,
                Status = ListingStatus.Active,
                AverageRating = 4.9,
                ReviewCount = 33,
                IsFeatured = true,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1522673607200-8364bbe0723a?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000007"),
                OwnerId = owner2.Id,
                Title = "Stand-Up Paddleboard — Bentota Lagoon",
                Description = "Inflatable SUP with paddle, pump, and ankle leash. Calm waters of Bentota lagoon make this ideal for sunrise paddles and yoga sessions. Life jacket included.",
                Category = "Sports Equipment",
                PricePerDay = 2500m,
                Deposit = 15000m,
                Location = "Bentota, Southern Province",
                City = "Galle",
                State = "Southern",
                Latitude = 6.4210,
                Longitude = 79.9956,
                Status = ListingStatus.Active,
                AverageRating = 4.6,
                ReviewCount = 17,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1530521954074-e64f2470fa8e?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000008"),
                OwnerId = owner.Id,
                Title = "DJI RS 3 Gimbal Stabilizer — Colombo Content Creators",
                Description = "Professional 3-axis gimbal for smooth walk-through videos of Colombo street food, boutique hotels, and product shoots. Supports most mirrorless cameras up to 3kg.",
                Category = "Cameras",
                PricePerDay = 3800m,
                Deposit = 25000m,
                Location = "Colombo 05, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.8820,
                Longitude = 79.8670,
                Status = ListingStatus.Active,
                AverageRating = 4.8,
                ReviewCount = 21,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1598488035139-bdbb2231d799?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000009"),
                OwnerId = owner.Id,
                Title = "2-Person Rooftop Tent — Horton Plains & Tea Country",
                Description = "Roof-top tent with ladder, mattress, and LED lantern. Popular with 4x4 owners camping near Horton Plains or Gregory Lake. Mounting rails fit most roof racks.",
                Category = "Camping Gear",
                PricePerDay = 6500m,
                Deposit = 40000m,
                Location = "Nuwara Eliya, Central Province",
                City = "Nuwara Eliya",
                State = "Central",
                Latitude = 6.9600,
                Longitude = 80.7800,
                Status = ListingStatus.Active,
                AverageRating = 4.7,
                ReviewCount = 13,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1504851149312-7a075b496cc7?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000010"),
                OwnerId = owner2.Id,
                Title = "Snorkelling Set — Hikkaduwa Coral Reef",
                Description = "Mask, snorkel, fins (sizes S–L), and reef-safe sunscreen tips sheet. Explore Hikkaduwa Marine Sanctuary at your own pace. Rinse tank provided on return.",
                Category = "Sports Equipment",
                PricePerDay = 1200m,
                Deposit = 5000m,
                Location = "Hikkaduwa, Southern Province",
                City = "Galle",
                State = "Southern",
                Latitude = 6.1400,
                Longitude = 80.1010,
                Status = ListingStatus.Active,
                AverageRating = 4.5,
                ReviewCount = 45,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000011"),
                OwnerId = owner2.Id,
                Title = "1.5 Ton Portable Air Conditioner — Colombo Events",
                Description = "Mobile AC unit with exhaust hose and window kit. Saves the day for enclosed wedding halls and office functions during April heat. Requires standard power outlet.",
                Category = "Home Appliances",
                PricePerDay = 4200m,
                Deposit = 20000m,
                Location = "Colombo 04, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.8980,
                Longitude = 79.8600,
                Status = ListingStatus.Active,
                AverageRating = 4.4,
                ReviewCount = 10,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1631545806608-43b378b2a4e8?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            },
            new()
            {
                Id = Guid.Parse("11000000-0000-0000-0000-000000000012"),
                OwnerId = owner.Id,
                Title = "Sony FX3 Cinema Camera — TV Drama & Documentary",
                Description = "Compact cinema line camera with XLR handle, 35mm f/1.8 lens, and 128GB CFexpress card. Used on local teledrama shoots in Colombo studios. Full-frame 4K 120fps capability.",
                Category = "Cameras",
                PricePerDay = 14000m,
                Deposit = 95000m,
                Location = "Colombo 08, Western Province",
                City = "Colombo",
                State = "Western",
                Latitude = 6.9140,
                Longitude = 79.8720,
                Status = ListingStatus.Active,
                AverageRating = 4.9,
                ReviewCount = 7,
                Images =
                [
                    new ListingImage { BlobUrl = "https://images.unsplash.com/photo-1601784551445-20e9ecf3bd81?w=600&q=80", IsPrimary = true, SortOrder = 0 }
                ]
            }
        };

        foreach (var listing in listings.Where(l => l.Status == ListingStatus.Active))
        {
            for (var d = today; d <= availEnd; d = d.AddDays(1))
                listing.Availability.Add(new ListingAvailability { Date = d, IsAvailable = true });
        }

        context.Listings.AddRange(listings);

        var cameraListing = listings[0];
        var toolListing = listings[1];

        var rentals = new List<Rental>
        {
            new()
            {
                Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"), // r වෙනුවට e ආදේශ කරන ලදී
                ListingId = cameraListing.Id,
                RenterId = renter.Id,
                StartDate = today.AddDays(3),
                EndDate = today.AddDays(6),
                Status = RentalStatus.Requested,
                TotalPrice = 8500m * 4,
                DepositAmount = 50000m,
                Message = "Need for a wedding shoot in Kandy."
            },
            new()
            {
                Id = Guid.Parse("e2222222-2222-2222-2222-222222222222"),
                ListingId = toolListing.Id,
                RenterId = renter.Id,
                StartDate = today.AddDays(-10),
                EndDate = today.AddDays(-7),
                Status = RentalStatus.HandedOver,
                TotalPrice = 3500m * 4,
                DepositAmount = 15000m,
                ApprovedAt = DateTime.UtcNow.AddDays(-12)
            },
            new()
            {
                Id = Guid.Parse("e3333333-3333-3333-3333-333333333333"),
                ListingId = listings[3].Id,
                RenterId = renter.Id,
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(-27),
                Status = RentalStatus.Reviewed,
                TotalPrice = 6500m * 4,
                DepositAmount = 35000m,
                CompletedAt = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                Id = Guid.Parse("e4444444-4444-4444-4444-444444444444"),
                ListingId = listings[4].Id,
                RenterId = renter.Id,
                StartDate = today.AddDays(-60),
                EndDate = today.AddDays(-58),
                Status = RentalStatus.Rejected,
                TotalPrice = 4000m * 3,
                DepositAmount = 18000m,
                OwnerNotes = "Item unavailable during requested dates."
            },
            new()
            {
                Id = Guid.Parse("e5555555-5555-5555-5555-555555555555"),
                ListingId = cameraListing.Id,
                RenterId = renter.Id,
                StartDate = today.AddDays(-90),
                EndDate = today.AddDays(-87),
                Status = RentalStatus.Reviewed,
                TotalPrice = 8500m * 4,
                DepositAmount = 50000m,
                CompletedAt = DateTime.UtcNow.AddMonths(-3)
            }
        };

        context.Rentals.AddRange(rentals);

        context.Reviews.Add(new Review
        {
            RentalId = rentals[2].Id,
            ReviewerId = renter.Id,
            RevieweeId = owner2.Id,
            Rating = 5,
            Comment = "Projector was perfect for our event!",
            IsOwnerReview = false
        });

        context.UserReports.AddRange(
            new UserReport
            {
                ReporterId = renter.Id,
                ReportedListingId = listings[7].Id,
                Reason = "Suspicious pricing",
                Description = "Price seems too low for this item."
            },
            new UserReport
            {
                ReporterId = admin.Id,
                ReportedUserId = owner2.Id,
                Reason = "Late return",
                Description = "Previous renter reported late return.",
                IsResolved = true
            }
        );

        context.Notifications.AddRange(
            new Notification
            {
                UserId = owner.Id,
                Type = NotificationType.BookingRequest,
                Title = "New rental request",
                Message = "Ryan Renter requested your Sony A7 IV camera kit.",
                ActionUrl = "/owner/dashboard"
            },
            new Notification
            {
                UserId = renter.Id,
                Type = NotificationType.System,
                Title = "Welcome to RentThings!",
                Message = "Browse thousands of items to rent near you.",
                ActionUrl = "/search"
            }
        );

        await context.SaveChangesAsync();
    }
}