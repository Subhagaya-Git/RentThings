namespace RentThings.Api.Services.Azure;

public interface IAiServicesClient
{
    Task<AiListingSuggestion> GenerateListingFromImageAsync(Stream imageStream, string? hint, CancellationToken ct = default);
    Task<AiChatResponse> ChatAsync(string message, string? conversationId, CancellationToken ct = default);
}

public record AiListingSuggestion(
    string Title,
    string Description,
    string Category,
    IReadOnlyList<string> RentalTips,
    IReadOnlyList<string> SuggestedCategories);

public record AiChatResponse(string Reply, string ConversationId);
