namespace RentThings.Api.Services.Azure;

public interface IBlobStorageService
{
    Task<string> UploadListingImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> UploadProfileImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> UploadVerificationDocAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteBlobAsync(string blobUrl, CancellationToken ct = default);
    Task<string> GetOptimizedThumbnailUrlAsync(string blobUrl, CancellationToken ct = default);
}
