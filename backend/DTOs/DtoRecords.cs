namespace RentThings.Api.DTOs;

public record UserDto(Guid Id, string Email, string DisplayName, string? ProfileImageUrl, string Role, int TrustScore, string TrustLevel, bool IsVerified, string? Location);
public record UserProfileDto(Guid Id, string Email, string DisplayName, string? PhoneNumber, string? ProfileImageUrl, string Role, int TrustScore, string TrustLevel, bool IsVerified, string? Location, string? Bio, DateTime CreatedAt);

public record CategoryDto(Guid Id, string Name, string Slug, string? Icon, string? Description, int ItemCount);

public record ListingImageDto(Guid Id, string BlobUrl, string? ThumbnailUrl, bool IsPrimary, int SortOrder, decimal? VisionScore, string? VisionIssues);
public record ListingDto(
    Guid Id, string Title, string Description, decimal PricePerDay, decimal DepositAmount,
    string Location, string Status, decimal AverageRating, int ReviewCount, bool IsFeatured,
    CategoryDto Category, UserDto Owner, IEnumerable<ListingImageDto> Images);

public record ListingSummaryDto(
    Guid Id, string Title, decimal PricePerDay, string Location, decimal AverageRating,
    int ReviewCount, bool IsFeatured, string? PrimaryImageUrl, CategoryDto Category);

public record CreateListingRequest(string Title, string Description, Guid CategoryId, decimal PricePerDay, decimal DepositAmount, string Location);
public record UpdateListingRequest(string Title, string Description, decimal PricePerDay, decimal DepositAmount, string Location);

public record RentalDto(
    Guid Id, Guid ListingId, string ListingTitle, string? ListingImageUrl,
    DateOnly StartDate, DateOnly EndDate, decimal TotalPrice, decimal DepositAmount,
    string Status, string PaymentStatus, UserDto Renter, UserDto Owner, DateTime CreatedAt);

public record CreateRentalRequest(Guid ListingId, DateOnly StartDate, DateOnly EndDate, string? RenterNotes);
public record UpdateRentalStatusRequest(string Status, string? Notes);

public record ReviewDto(Guid Id, int Rating, string? Comment, string ReviewType, UserDto Reviewer, DateTime CreatedAt);
public record CreateReviewRequest(Guid RentalId, int Rating, string? Comment, string ReviewType);

public record NotificationDto(Guid Id, string Title, string Message, string Type, bool IsRead, Guid? RelatedEntityId, DateTime CreatedAt);

public record SearchListingsRequest(string? Query, Guid? CategoryId, decimal? MinPrice, decimal? MaxPrice, string? Location, decimal? MinRating, DateOnly? AvailableFrom, DateOnly? AvailableTo, string SortBy = "relevance", int Page = 1, int PageSize = 12);
public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);

public record AiListingGenerationRequest(string? ImageBase64, string? Hint);
public record AiListingGenerationResponse(string Title, string Description, IEnumerable<string> CategorySuggestions, IEnumerable<string> RentalTips);

public record VisionValidationResult(bool Passed, decimal Score, IEnumerable<string> Issues, string Recommendation);
public record AiChatRequest(string Message, string? ConversationId);
public record AiChatResponse(string Reply, string ConversationId);

public record AdminAnalyticsDto(int TotalUsers, int TotalListings, int TotalRentals, int ActiveRentals, decimal TotalRevenue, IEnumerable<ChartDataPoint> RentalsByMonth, IEnumerable<ChartDataPoint> UsersByMonth);
public record ChartDataPoint(string Label, decimal Value);

public record TrustScoreDto(int Score, string Level, IEnumerable<string> Factors);

public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);

public record PlatformStatsDto(int TotalUsers, int TotalListings, int CompletedRentals, decimal AverageRating);
