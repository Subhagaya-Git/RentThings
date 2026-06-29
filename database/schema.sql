-- RentThings Azure SQL Database Schema
-- Run against Azure SQL Database or LocalDB for development

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(256) NOT NULL UNIQUE,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Bio NVARCHAR(1000) NULL,
        Location NVARCHAR(200) NULL,
        AvatarUrl NVARCHAR(500) NULL,
        EntraObjectId NVARCHAR(100) NULL,
        Role INT NOT NULL DEFAULT 0,
        TrustScore INT NOT NULL DEFAULT 50,
        TrustLevel INT NOT NULL DEFAULT 1,
        IsVerified BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Listings')
BEGIN
    CREATE TABLE Listings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OwnerId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        PricePerDay DECIMAL(18,2) NOT NULL,
        Deposit DECIMAL(18,2) NOT NULL,
        Location NVARCHAR(200) NOT NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(50) NULL,
        Status INT NOT NULL DEFAULT 0,
        AverageRating FLOAT NOT NULL DEFAULT 0,
        ReviewCount INT NOT NULL DEFAULT 0,
        ViewCount INT NOT NULL DEFAULT 0,
        IsFeatured BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NULL
    );
    CREATE INDEX IX_Listings_Category ON Listings(Category);
    CREATE INDEX IX_Listings_Status ON Listings(Status);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ListingImages')
BEGIN
    CREATE TABLE ListingImages (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ListingId UNIQUEIDENTIFIER NOT NULL REFERENCES Listings(Id) ON DELETE CASCADE,
        BlobUrl NVARCHAR(500) NOT NULL,
        ThumbnailUrl NVARCHAR(500) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        IsPrimary BIT NOT NULL DEFAULT 0,
        PassedValidation BIT NOT NULL DEFAULT 1,
        ValidationNotes NVARCHAR(500) NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rentals')
BEGIN
    CREATE TABLE Rentals (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ListingId UNIQUEIDENTIFIER NOT NULL REFERENCES Listings(Id),
        RenterId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        Status INT NOT NULL DEFAULT 0,
        TotalPrice DECIMAL(18,2) NOT NULL,
        DepositAmount DECIMAL(18,2) NOT NULL,
        Message NVARCHAR(1000) NULL,
        OwnerNotes NVARCHAR(1000) NULL,
        IsLateReturn BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ApprovedAt DATETIME2 NULL,
        CompletedAt DATETIME2 NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE Reviews (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        RentalId UNIQUEIDENTIFIER NOT NULL REFERENCES Rentals(Id),
        ReviewerId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        RevieweeId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
        Comment NVARCHAR(2000) NOT NULL,
        IsOwnerReview BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        Type INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        ActionUrl NVARCHAR(500) NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_Notifications_UserId ON Notifications(UserId, IsRead);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Favorites')
BEGIN
    CREATE TABLE Favorites (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        ListingId UNIQUEIDENTIFIER NOT NULL REFERENCES Listings(Id),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Favorites UNIQUE (UserId, ListingId)
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserReports')
BEGIN
    CREATE TABLE UserReports (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ReporterId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(Id),
        ReportedUserId UNIQUEIDENTIFIER NULL REFERENCES Users(Id),
        ReportedListingId UNIQUEIDENTIFIER NULL REFERENCES Listings(Id),
        Reason NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NOT NULL,
        IsResolved BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
