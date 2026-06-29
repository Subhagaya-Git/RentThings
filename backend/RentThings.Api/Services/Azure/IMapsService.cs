namespace RentThings.Api.Services.Azure;

public interface IMapsService
{
    Task<GeocodeResult?> GeocodeAsync(string address, CancellationToken ct = default);
    double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2);
    string GetStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 300);
}

public record GeocodeResult(double Latitude, double Longitude, string FormattedAddress);
