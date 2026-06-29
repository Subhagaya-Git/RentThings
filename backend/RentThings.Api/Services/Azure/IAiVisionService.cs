namespace RentThings.Api.Services.Azure;

public interface IAiVisionService
{
    Task<ImageValidationResult> ValidateListingImageAsync(Stream imageStream, CancellationToken ct = default);
}

public record ImageValidationResult(
    bool IsValid,
    bool HasInappropriateContent,
    bool IsLowQuality,
    bool IsBlurry,
    bool HasVisibleObject,
    double QualityScore,
    IReadOnlyList<string> Issues,
    IReadOnlyList<string> Recommendations,
    string Category = "",
    string Subcategory = "",
    IReadOnlyList<string>? Tags = null,
    double Confidence = 0,
    bool Flagged = false);
