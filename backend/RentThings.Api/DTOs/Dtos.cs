using RentThings.Api.Models;

namespace RentThings.Api.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? Bio,
    string? Location,
    string? AvatarUrl,
    string Role,
    int TrustScore,
    string TrustLevel,
    bool IsVerified,
    bool IsActive,
    DateTime CreatedAt);

public record AuthRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role = "Renter");
public record AuthResponse(string Token, UserDto User);

public record ListingDto(
    Guid Id,
    Guid OwnerId,
    string OwnerName,
    string Title,
    string Description,
    string Category,
    decimal PricePerDay,
    decimal Deposit,
    string Location,
    string? City,
    string? State,
    string Status,
    double AverageRating,
    int ReviewCount,
    bool IsFeatured,
    IReadOnlyList<ListingImageDto> Images,
    DateTime CreatedAt,
    double? Latitude = null,
    double? Longitude = null,
    double? DistanceKm = null,
    string? MapImageUrl = null);

public record ListingImageDto(Guid Id, string Url, string? ThumbnailUrl, bool IsPrimary, bool PassedValidation, string? ValidationNotes);

public record CreateListingRequest(
    string Title,
    string Description,
    string Category,
    decimal PricePerDay,
    decimal Deposit,
    string Location,
    string? City,
    string? State,
    DateOnly? AvailableFrom = null,
    DateOnly? AvailableTo = null);

public record ListingSearchParams(
    string? Query,
    string? Category,
    string? Location,
    decimal? MinPrice,
    decimal? MaxPrice,
    double? MinRating,
    DateOnly? AvailableFrom,
    DateOnly? AvailableTo,
    double? Latitude,
    double? Longitude,
    double? RadiusKm,
    string SortBy = "featured",
    int Page = 1,
    int PageSize = 12);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public record RentalDto(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    string? ListingImage,
    Guid RenterId,
    string RenterName,
    Guid OwnerId,
    string OwnerName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    decimal TotalPrice,
    decimal DepositAmount,
    string? Message,
    string? RejectionReason,
    DateTime CreatedAt);

public record CreateRentalRequest(Guid ListingId, DateOnly StartDate, DateOnly EndDate, string? Message);
public record UpdateRentalStatusRequest(string Status, string? Notes);

public record ReviewDto(
    Guid Id,
    Guid RentalId,
    Guid ReviewerId,
    string ReviewerName,
    int Rating,
    string Comment,
    bool IsOwnerReview,
    DateTime CreatedAt);

public record CreateReviewRequest(Guid RentalId, int Rating, string Comment);

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? ActionUrl,
    bool IsRead,
    DateTime CreatedAt);

public record AdminStatsDto(
    int TotalUsers,
    int TotalListings,
    int ActiveRentals,
    int CompletedRentals,
    decimal TotalRevenue,
    int FlaggedListings,
    int PendingReports,
    IReadOnlyList<MonthlyStatDto> MonthlyRentals,
    IReadOnlyList<CategoryStatDto> RentalsByCategory,
    IReadOnlyList<StatusStatDto> RentalsByStatus);

public record CategoryStatDto(string Category, int Count);
public record StatusStatDto(string Status, int Count);

public record OwnerDashboardDto(
    int ActiveListings,
    int InactiveListings,
    int PendingRequests,
    int ActiveRentals,
    decimal TotalEarnings,
    IReadOnlyList<ListingDto> Listings,
    IReadOnlyList<RentalDto> Requests,
    IReadOnlyList<RentalDto> ActiveRentalsList);

public record ReportDto(
    Guid Id,
    Guid ReporterId,
    string ReporterName,
    Guid? ReportedUserId,
    string? ReportedUserName,
    Guid? ReportedListingId,
    string? ReportedListingTitle,
    string Reason,
    string Description,
    bool IsResolved,
    DateTime CreatedAt);

public record UpdateTrustScoreRequest(int TrustScore);
public record SuspendUserRequest(bool IsActive);
public record UpdateListingStatusRequest(string Status);

public record MonthlyStatDto(string Month, int Count, decimal Revenue);

public record PlatformStatsDto(
    int TotalListings,
    int TotalRentals,
    int HappyRenters,
    double AverageRating);

public record ImageValidationDto(
    bool IsValid,
    bool HasInappropriateContent,
    bool IsLowQuality,
    bool IsBlurry,
    bool HasVisibleObject,
    double QualityScore,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Recommendations,
    string Category,
    string Subcategory,
    IReadOnlyList<string> Tags,
    double Confidence,
    bool Flagged);

public record AiListingSuggestionDto(
    string Title,
    string Description,
    string Category,
    IReadOnlyList<string> RentalTips,
    IReadOnlyList<string> SuggestedCategories);

public record AiChatRequest(string Message, string? ConversationId);
public record AiChatResponseDto(string Reply, string ConversationId);

public record GeocodeResultDto(double Latitude, double Longitude, string FormattedAddress);
public record MapStaticUrlDto(string Url);

public record UpdateProfileRequest(string? FirstName, string? LastName, string? Phone, string? Bio, string? Location);
