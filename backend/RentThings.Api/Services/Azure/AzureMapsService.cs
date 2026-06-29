using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RentThings.Api.Configuration;

namespace RentThings.Api.Services.Azure;

public class AzureMapsService : IMapsService
{
    private readonly MapsSettings _settings;
    private readonly HttpClient _http;
    private readonly ILogger<AzureMapsService> _logger;

    public AzureMapsService(HttpClient http, IOptions<AzureSettings> options, ILogger<AzureMapsService> logger)
    {
        _http = http;
        _settings = options.Value.Maps;
        _logger = logger;
    }

    public async Task<GeocodeResult?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        var url = $"https://atlas.microsoft.com/search/address/json?api-version=1.0&subscription-key={_settings.SubscriptionKey}&query={Uri.EscapeDataString(address)}";
        try
        {
            var response = await _http.GetFromJsonAsync<MapsSearchResponse>(url, ct);
            var result = response?.Results?.FirstOrDefault();
            if (result?.Position is null) return null;

            return new GeocodeResult(
                result.Position.Lat,
                result.Position.Lon,
                result.Address?.FreeformAddress ?? address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Maps geocode failed for {Address}", address);
            return null;
        }
    }

    public double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public string GetStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 300)
        => $"https://atlas.microsoft.com/map/static/png?api-version=2024-04-01&subscription-key={_settings.SubscriptionKey}" +
           $"&center={longitude},{latitude}&zoom={zoom}&width={width}&height={height}" +
           $"&pins=default|co0078D4||{longitude} {latitude}";

    private static double DegreesToRadians(double deg) => deg * Math.PI / 180;

    private sealed class MapsSearchResponse
    {
        [JsonPropertyName("results")]
        public List<MapsResult>? Results { get; set; }
    }

    private sealed class MapsResult
    {
        [JsonPropertyName("position")]
        public MapsPosition? Position { get; set; }

        [JsonPropertyName("address")]
        public MapsAddress? Address { get; set; }
    }

    private sealed class MapsPosition
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    private sealed class MapsAddress
    {
        [JsonPropertyName("freeformAddress")]
        public string? FreeformAddress { get; set; }
    }
}
