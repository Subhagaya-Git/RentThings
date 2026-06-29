namespace RentThings.Api.Services.Azure;

public class MockMapsService : IMapsService
{
    private static readonly Dictionary<string, (double Lat, double Lon)> CityCoords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Portland, OR"] = (45.5152, -122.6784),
        ["Seattle, WA"] = (47.6062, -122.3321),
        ["San Francisco, CA"] = (37.7749, -122.4194),
        ["Los Angeles, CA"] = (34.0522, -118.2437),
        ["Denver, CO"] = (39.7392, -104.9903),
        ["Colombo"] = (6.9271, 79.8612),
        ["Negombo"] = (7.2088, 79.8358),
        ["Kandy"] = (7.2906, 80.6337),
        ["Galle"] = (6.0535, 80.2210),
        ["Kalutara"] = (6.5854, 79.9607),
    };

    public Task<GeocodeResult?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        foreach (var (city, coords) in CityCoords)
        {
            if (address.Contains(city, StringComparison.OrdinalIgnoreCase) ||
                city.Contains(address, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<GeocodeResult?>(new GeocodeResult(coords.Lat, coords.Lon, city));
        }
        return Task.FromResult<GeocodeResult?>(new GeocodeResult(47.6062, -122.3321, address));
    }

    public double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public string GetStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 300)
        => $"https://maps.geoapify.com/v1/staticmap?style=osm-bright&width={width}&height={height}&center=lonlat:{longitude},{latitude}&zoom={zoom}&marker=lonlat:{longitude},{latitude};color:%231f8fa6;size:medium&apiKey=placeholder";
}
