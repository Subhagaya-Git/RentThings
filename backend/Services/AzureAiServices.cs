using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;
using RentThings.Api.DTOs;

namespace RentThings.Api.Services;

public class AiListingService(IOptions<AzureAiSettings> options, ILogger<AiListingService> logger) : IAiListingService
{
    public Task<AiListingGenerationResponse> GenerateListingFromImageAsync(AiListingGenerationRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("AI listing generation (mock) — configure Azure:Ai:OpenAiEndpoint for production");
        var response = new AiListingGenerationResponse(
            Title: "Professional DSLR Camera Kit",
            Description: "Complete camera rental package including body, 24-70mm lens, spare battery, and carrying case. Perfect for events, portraits, and content creation. Well-maintained and ready to shoot.",
            CategorySuggestions: ["Cameras & Photography", "Event Equipment", "Electronics"],
            RentalTips: [
                "Include a quick-start guide for renters unfamiliar with the model",
                "Require ID verification for high-value equipment",
                "Offer optional insurance for peace of mind",
                "Clean lenses between each rental"
            ]);
        return Task.FromResult(response);
    }
}

public class AiVisionService(ILogger<AiVisionService> logger) : IAiVisionService
{
    public Task<VisionValidationResult> ValidateImageAsync(Stream imageStream, CancellationToken ct = default)
    {
        logger.LogInformation("AI Vision validation (mock)");
        return Task.FromResult(new VisionValidationResult(
            Passed: true,
            Score: 87.5m,
            Issues: [],
            Recommendation: "Image quality is good. Object is clearly visible and well-lit."));
    }
}

public class AiChatService(ILogger<AiChatService> logger) : IAiChatService
{
    private static readonly Dictionary<string, List<string>> Conversations = new();

    public Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("AI chat (mock): {Message}", request.Message);
        var convId = request.ConversationId ?? Guid.NewGuid().ToString();
        var reply = request.Message.ToLowerInvariant() switch
        {
            var m when m.Contains("cancel") => "You can cancel a pending rental request from your dashboard before the owner approves it. Approved rentals may incur cancellation fees per the owner's policy.",
            var m when m.Contains("deposit") => "Deposits are held during the rental period and refunded within 3-5 business days after the item is returned in good condition.",
            var m when m.Contains("trust") => "Your trust score (0-100) reflects completed rentals, reviews, verification status, and return history. Higher scores unlock Gold and Platinum badges!",
            _ => "I'm RentThings AI Assistant. I can help with bookings, deposits, trust scores, listing tips, and platform policies. What would you like to know?"
        };
        return Task.FromResult(new AiChatResponse(reply, convId));
    }
}

public class CommunicationService(ILogger<CommunicationService> logger) : ICommunicationService
{
    public Task SendBookingConfirmationAsync(string email, string phone, RentalDto rental, CancellationToken ct = default)
    {
        logger.LogInformation("Mock email/SMS booking confirmation to {Email}", email);
        return Task.CompletedTask;
    }

    public Task SendApprovalNotificationAsync(string email, RentalDto rental, CancellationToken ct = default)
    {
        logger.LogInformation("Mock approval notification to {Email}", email);
        return Task.CompletedTask;
    }

    public Task SendReturnReminderAsync(string email, string phone, RentalDto rental, CancellationToken ct = default)
    {
        logger.LogInformation("Mock return reminder to {Email}", email);
        return Task.CompletedTask;
    }

    public Task SendReviewNotificationAsync(string email, string reviewerName, CancellationToken ct = default)
    {
        logger.LogInformation("Mock review notification to {Email}", email);
        return Task.CompletedTask;
    }
}
