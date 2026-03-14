namespace Service.Services.Interfaces;

public interface IGeocodingService
{
    Task<(decimal? Latitude, decimal? Longitude)> GeocodeAddressAsync(string address);
}
