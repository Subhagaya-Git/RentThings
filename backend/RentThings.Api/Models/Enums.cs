namespace RentThings.Api.Models;

public enum UserRole
{
    Renter = 0,
    Owner = 1,
    Admin = 2
}

public enum TrustLevel
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

public enum ListingStatus
{
    Draft = 0,
    PendingReview = 1,
    Active = 2,
    Inactive = 3,
    Flagged = 4,
    Expired = 5
}

public enum RentalStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    PaymentPending = 3,
    Active = 4,
    Completed = 5,
    Cancelled = 6,
    HandedOver = 7,
    Returned = 8,
    Reviewed = 9
}

public enum NotificationType
{
    BookingRequest = 0,
    BookingApproved = 1,
    BookingRejected = 2,
    ReturnReminder = 3,
    RentalCompleted = 4,
    NewReview = 5,
    ListingFlagged = 6,
    System = 7
}
