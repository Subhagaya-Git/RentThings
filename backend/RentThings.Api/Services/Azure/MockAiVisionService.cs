namespace RentThings.Api.Services.Azure;

public class MockAiVisionService(ILogger<MockAiVisionService> logger) : IAiVisionService
{
    public Task<ImageValidationResult> ValidateListingImageAsync(Stream imageStream, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock AI Vision] Validating listing image");
        return Task.FromResult(new ImageValidationResult(
            IsValid: true,
            HasInappropriateContent: false,
            IsLowQuality: false,
            IsBlurry: false,
            HasVisibleObject: true,
            QualityScore: 0.92,
            Issues: [],
            Recommendations:
            [
                "Image quality looks great!",
                "Consider adding a photo showing scale or included accessories"
            ],
            Category: "Electronics",
            Subcategory: "device",
            Tags: ["item", "product", "rental"],
            Confidence: 0.92,
            Flagged: false));
    }
}
