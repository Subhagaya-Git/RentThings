using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;

namespace RentThings.Api.Services.Azure;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobStorageSettings _settings;
    private readonly BlobServiceClient _client;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IOptions<AzureSettings> options, ILogger<AzureBlobStorageService> logger)
    {
        _settings = options.Value.BlobStorage;
        _logger = logger;
        _client = new BlobServiceClient(_settings.ConnectionString);
    }

    public Task<string> UploadListingImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
        => UploadAsync(stream, fileName, contentType, _settings.ContainerName, ct);

    public Task<string> UploadProfileImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
        => UploadAsync(stream, fileName, contentType, _settings.ProfileContainer, ct);

    public Task<string> UploadVerificationDocAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
        => UploadAsync(stream, fileName, contentType, _settings.VerificationContainer, ct);

    public async Task DeleteBlobAsync(string blobUrl, CancellationToken ct = default)
    {
        try
        {
            var uri = new Uri(blobUrl);
            var blobClient = new BlobClient(uri);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
            _logger.LogInformation("Deleted blob: {Url}", blobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob: {Url}", blobUrl);
        }
    }

    public Task<string> GetOptimizedThumbnailUrlAsync(string blobUrl, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(_settings.CdnBaseUrl))
        {
            var cdnUrl = ToCdnUrl(blobUrl);
            return Task.FromResult($"{cdnUrl}?w=400&h=300&fit=crop");
        }
        return Task.FromResult(blobUrl);
    }

    private async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string containerName, CancellationToken ct)
    {
        var container = _client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var blob = container.GetBlobClient(safeName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, ct);

        var url = ToCdnUrl(blob.Uri.ToString());
        _logger.LogInformation("Uploaded blob to {Container}: {Url}", containerName, url);
        return url;
    }

    private string ToCdnUrl(string blobUrl)
    {
        if (string.IsNullOrWhiteSpace(_settings.CdnBaseUrl)) return blobUrl;

        var uri = new Uri(blobUrl);
        var path = uri.AbsolutePath; // /container/blobname
        return $"{_settings.CdnBaseUrl.TrimEnd('/')}{path}";
    }
}
