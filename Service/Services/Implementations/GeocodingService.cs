using System.Text.Json;
using Microsoft.Extensions.Logging;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingService> _logger;
    private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/search";

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TheFlowerApp/1.0");
    }

    public async Task<(decimal? Latitude, decimal? Longitude)> GeocodeAddressAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return (null, null);

            var query = new Uri($"{NominatimBaseUrl}?q={Uri.EscapeDataString(address)}&format=json&limit=1");
            var response = await _httpClient.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Geocoding API error: {response.StatusCode}");
                return (null, null);
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.GetArrayLength() == 0)
            {
                _logger.LogWarning($"No results found for address: {address}");
                return (null, null);
            }

            var firstResult = root[0];
            if (decimal.TryParse(firstResult.GetProperty("lat").GetString(), out var lat) &&
                decimal.TryParse(firstResult.GetProperty("lon").GetString(), out var lon))
            {
                _logger.LogInformation($"Geocoded address '{address}' to ({lat}, {lon})");
                return (lat, lon);
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error geocoding address '{address}': {ex.Message}");
            return (null, null);
        }
    }
}
