using RentThings.Api.DTOs;
using RentThings.Api.Models;

namespace RentThings.Api.Services;

public interface IBlobStorageService
{
    Task<string> UploadListingImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> UploadProfileImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> GenerateThumbnailAsync(string blobUrl, CancellationToken ct = default);
    Task DeleteBlobAsync(string blobUrl, CancellationToken ct = default);
}

public interface IEntraIdAuthService
{
    Task<AuthResponse?> AuthenticateAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserDto?> GetUserFromTokenAsync(string token, CancellationToken ct = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
}

public interface IAiListingService
{
    Task<AiListingGenerationResponse> GenerateListingFromImageAsync(AiListingGenerationRequest request, CancellationToken ct = default);
}

public interface IAiVisionService
{
    Task<VisionValidationResult> ValidateImageAsync(Stream imageStream, CancellationToken ct = default);
}

public interface IAiChatService
{
    Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken ct = default);
}

public interface ICommunicationService
{
    Task SendBookingConfirmationAsync(string email, string phone, RentalDto rental, CancellationToken ct = default);
    Task SendApprovalNotificationAsync(string email, RentalDto rental, CancellationToken ct = default);
    Task SendReturnReminderAsync(string email, string phone, RentalDto rental, CancellationToken ct = default);
    Task SendReviewNotificationAsync(string email, string reviewerName, CancellationToken ct = default);
}

public interface ITrustScoreService
{
    Task RecalculateScoreAsync(Guid userId, CancellationToken ct = default);
    TrustScoreDto GetTrustScore(User user);
    TrustLevel CalculateLevel(int score);
}

public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedId = null, CancellationToken ct = default);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
}
