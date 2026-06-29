namespace RentThings.Api.Services.Azure;

public class MockAiServicesClient(ILogger<MockAiServicesClient> logger) : IAiServicesClient
{
    public Task<AiListingSuggestion> GenerateListingFromImageAsync(Stream imageStream, string? hint, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock AI] Generating listing from image");
        var suggestion = new AiListingSuggestion(
            Title: hint ?? "Premium Rental Item",
            Description: "Well-maintained item available for short-term rental. Includes all standard accessories. Perfect for personal or professional use. Flexible pickup and return times available.",
            Category: "Electronics",
            RentalTips:
            [
                "Include clear photos from multiple angles",
                "Specify what's included in the rental",
                "Set a reasonable security deposit",
                "Describe any usage restrictions upfront"
            ],
            SuggestedCategories: ["Cameras", "Electronics", "Event Equipment", "Home Appliances"]);

        return Task.FromResult(suggestion);
    }

    public Task<AiChatResponse> ChatAsync(string message, string? conversationId, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock AI Chat] Message: {Message}", message);
        var id = conversationId ?? Guid.NewGuid().ToString();
        var reply = message.ToLowerInvariant() switch
        {
            var m when m.Contains("cancel") => "You can cancel a pending rental request from your dashboard under Active Rentals. Approved rentals may have cancellation fees based on the owner's policy.",
            var m when m.Contains("deposit") => "Deposits are held securely and returned within 48 hours after the item is returned in good condition. Damage claims are handled through our dispute resolution process.",
            var m when m.Contains("trust") => "Your trust score (0-100) reflects completed rentals, reviews, verification status, and return history. Higher scores unlock better visibility and lower deposits.",
            _ => "I'm RentThings AI Assistant. I can help with bookings, listings, trust scores, deposits, and returns. What would you like to know?"
        };

        return Task.FromResult(new AiChatResponse(reply, id));
    }
}
