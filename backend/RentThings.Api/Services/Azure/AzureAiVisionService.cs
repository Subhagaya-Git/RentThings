using Azure;
using Azure.AI.ContentSafety;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;

namespace RentThings.Api.Services.Azure;

public class AzureAiVisionService : IAiVisionService
{
    private readonly ImageAnalysisClient _visionClient;
    private readonly ContentSafetyClient? _safetyClient;
    private readonly ILogger<AzureAiVisionService> _logger;

    private static readonly Dictionary<string, string> TagToCategory = new(StringComparer.OrdinalIgnoreCase)
    {
        ["camera"] = "Cameras", ["photography"] = "Cameras", ["lens"] = "Cameras",
        ["drill"] = "Power Tools", ["tool"] = "Power Tools", ["saw"] = "Power Tools",
        ["tent"] = "Camping Gear", ["camping"] = "Camping Gear", ["backpack"] = "Camping Gear",
        ["projector"] = "Event Equipment", ["speaker"] = "Speakers", ["audio"] = "Speakers",
        ["vacuum"] = "Home Appliances", ["appliance"] = "Home Appliances",
        ["bicycle"] = "Sports Equipment", ["sport"] = "Sports Equipment",
        ["laptop"] = "Electronics", ["computer"] = "Electronics", ["phone"] = "Electronics",
    };

    public AzureAiVisionService(IOptions<AzureSettings> options, ILogger<AzureAiVisionService> logger)
    {
        var settings = options.Value.AiVision;
        _logger = logger;
        _visionClient = new ImageAnalysisClient(
            new Uri(settings.Endpoint.TrimEnd('/')),
            new AzureKeyCredential(settings.ApiKey));

        // Content Safety uses same endpoint pattern when deployed as multi-service resource
        try
        {
            _safetyClient = new ContentSafetyClient(new Uri(settings.Endpoint.TrimEnd('/')), new AzureKeyCredential(settings.ApiKey));
        }
        catch
        {
            _safetyClient = null;
        }
    }

    public async Task<ImageValidationResult> ValidateListingImageAsync(Stream imageStream, CancellationToken ct = default)
    {
        var data = await BinaryData.FromStreamAsync(imageStream, ct);

        ImageAnalysisResult analysis;
        try
        {
            var response = await _visionClient.AnalyzeAsync(
                data,
                VisualFeatures.Tags | VisualFeatures.Caption | VisualFeatures.Objects,
                cancellationToken: ct);
            analysis = response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure AI Vision analysis failed");
            return new ImageValidationResult(
                IsValid: false, HasInappropriateContent: false, IsLowQuality: true,
                IsBlurry: false, HasVisibleObject: false, QualityScore: 0,
                Issues: ["Vision API call failed"], Recommendations: ["Try uploading again"]);
        }

        var tags = analysis.Tags?.Values?.Select(t => t.Name).ToList() ?? [];
        var caption = analysis.Caption?.Text ?? "";
        var hasObject = (analysis.Objects?.Values?.Count ?? 0) > 0 || tags.Count > 0;
        var confidence = analysis.Tags?.Values?.MaxBy(t => t.Confidence)?.Confidence ?? 0.5;

        var (category, subcategory) = MapCategory(tags, caption);
        var flagged = await CheckInappropriateAsync(data, ct);
        var issues = new List<string>();
        var recommendations = new List<string>();

        if (flagged) issues.Add("Image flagged for inappropriate content");
        if (!hasObject) { issues.Add("No clear object detected"); recommendations.Add("Ensure the rental item is clearly visible"); }
        if (confidence < 0.3) { issues.Add("Low confidence in image analysis"); recommendations.Add("Use a well-lit photo with plain background"); }

        var qualityScore = Math.Clamp(confidence + (hasObject ? 0.3 : 0) + (tags.Count > 2 ? 0.1 : 0), 0, 1);
        var isValid = !flagged && hasObject && qualityScore >= 0.4;

        if (isValid) recommendations.Add($"Detected category: {category}");
        if (tags.Count > 0) recommendations.Add($"Tags: {string.Join(", ", tags.Take(5))}");

        return new ImageValidationResult(
            IsValid: isValid,
            HasInappropriateContent: flagged,
            IsLowQuality: qualityScore < 0.5,
            IsBlurry: false,
            HasVisibleObject: hasObject,
            QualityScore: qualityScore,
            Issues: issues,
            Recommendations: recommendations,
            Category: category,
            Subcategory: subcategory,
            Tags: tags,
            Confidence: confidence,
            Flagged: flagged);
    }

    private async Task<bool> CheckInappropriateAsync(BinaryData data, CancellationToken ct)
    {
        if (_safetyClient is null) return false;
        try
        {
            var result = await _safetyClient.AnalyzeImageAsync(data, cancellationToken: ct);
            var adult = result.Value.CategoriesAnalysis.FirstOrDefault(c => c.Category == ImageCategory.Hate);
            // Check all harmful categories
            return result.Value.CategoriesAnalysis.Any(c =>
                (c.Category == ImageCategory.Sexual ||
                 c.Category == ImageCategory.Violence ||
                 c.Category == ImageCategory.Hate) &&
                c.Severity >= 4);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Content Safety check skipped");
            return false;
        }
    }

    private static (string Category, string Subcategory) MapCategory(IReadOnlyList<string> tags, string caption)
    {
        var combined = string.Join(" ", tags.Concat([caption])).ToLowerInvariant();
        foreach (var (keyword, category) in TagToCategory)
        {
            if (combined.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return (category, keyword);
        }
        return tags.FirstOrDefault() is { } t ? ("Electronics", t) : ("General", "item");
    }
}
