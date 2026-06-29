using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;

namespace RentThings.Api.Services.Azure;

public class MockBlobStorageService(IOptions<AzureSettings> settings, ILogger<MockBlobStorageService> logger) : IBlobStorageService
{
    private readonly BlobStorageSettings _settings = settings.Value.BlobStorage;

    public Task<string> UploadListingImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var url = $"https://{_settings.ContainerName}.blob.core.windows.net/listings/{Guid.NewGuid()}-{fileName}";
        logger.LogInformation("[Mock Blob] Uploaded listing image: {Url}", url);
        return Task.FromResult(url);
    }

    public Task<string> UploadProfileImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var url = $"https://{_settings.ProfileContainer}.blob.core.windows.net/{Guid.NewGuid()}-{fileName}";
        logger.LogInformation("[Mock Blob] Uploaded profile image: {Url}", url);
        return Task.FromResult(url);
    }

    public Task<string> UploadVerificationDocAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var url = $"https://{_settings.VerificationContainer}.blob.core.windows.net/{Guid.NewGuid()}-{fileName}";
        logger.LogInformation("[Mock Blob] Uploaded verification doc: {Url}", url);
        return Task.FromResult(url);
    }

    public Task DeleteBlobAsync(string blobUrl, CancellationToken ct = default)
    {
        logger.LogInformation("[Mock Blob] Deleted: {Url}", blobUrl);
        return Task.CompletedTask;
    }

    public Task<string> GetOptimizedThumbnailUrlAsync(string blobUrl, CancellationToken ct = default)
        => Task.FromResult($"{blobUrl}?w=400&h=300&fit=crop");
}
