using Repository.Repositories;
using Service.DTOs.Maps;

namespace Service.Services;

public class StoreLocationService : IStoreLocationService
{
    private readonly IStoreLocationRepository _locationRepository;

    public StoreLocationService(IStoreLocationRepository locationRepository)
        => _locationRepository = locationRepository;

    public async Task<IEnumerable<StoreLocationDto>> GetAllAsync()
    {
        var locations = await _locationRepository.GetAllAsync();
        return locations.Select(l => new StoreLocationDto
        {
            LocationId = l.LocationId,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Address = l.Address
        });
    }

    public async Task<StoreLocationDto?> GetByIdAsync(int id)
    {
        var l = await _locationRepository.GetByIdAsync(id);
        if (l is null) return null;

        return new StoreLocationDto
        {
            LocationId = l.LocationId,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            Address = l.Address
        };
    }
}
