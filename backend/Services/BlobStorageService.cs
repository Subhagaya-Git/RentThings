using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;

namespace RentThings.Api.Services;

public class BlobStorageService(IOptions<AzureBlobSettings> options, ILogger<BlobStorageService> logger) : IBlobStorageService
{
    private readonly AzureBlobSettings _settings = options.Value;

    public Task<string> UploadListingImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_settings.ConnectionString))
        {
            var mockUrl = $"https://{_settings.AccountName ?? "rentthings"}.blob.core.windows.net/listings/{Guid.NewGuid()}/{fileName}";
            logger.LogInformation("Mock blob upload: {Url}", mockUrl);
            return Task.FromResult(mockUrl);
        }
        // Production: use BlobServiceClient to upload
        throw new NotImplementedException("Configure Azure:BlobStorage:ConnectionString for production uploads.");
    }

    public Task<string> UploadProfileImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var mockUrl = $"https://{_settings.AccountName ?? "rentthings"}.blob.core.windows.net/profiles/{Guid.NewGuid()}/{fileName}";
        logger.LogInformation("Mock profile upload: {Url}", mockUrl);
        return Task.FromResult(mockUrl);
    }

    public Task<string> GenerateThumbnailAsync(string blobUrl, CancellationToken ct = default)
        => Task.FromResult(blobUrl.Replace("/listings/", "/thumbnails/"));

    public Task DeleteBlobAsync(string blobUrl, CancellationToken ct = default) => Task.CompletedTask;
}
