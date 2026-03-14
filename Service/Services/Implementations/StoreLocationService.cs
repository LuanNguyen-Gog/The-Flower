using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Maps;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class StoreLocationService : IStoreLocationService
{
    private readonly IStoreLocationRepository _locationRepository;
    private readonly IGeocodingService _geocodingService;

    public StoreLocationService(IStoreLocationRepository locationRepository, IGeocodingService geocodingService)
    {
        _locationRepository = locationRepository;
        _geocodingService = geocodingService;
    }

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

    public async Task<StoreLocationDto> CreateAsync(CreateStoreLocationDto dto)
    {
        var (latitude, longitude) = await _geocodingService.GeocodeAddressAsync(dto.Address);

        var location = new StoreLocation
        {
            Address = dto.Address,
            Latitude = latitude,
            Longitude = longitude,
            Status = "Active"
        };

        var createdLocation = await _locationRepository.CreateAsync(location);

        return new StoreLocationDto
        {
            LocationId = createdLocation.LocationId,
            Latitude = createdLocation.Latitude,
            Longitude = createdLocation.Longitude,
            Address = createdLocation.Address
        };
    }

    public async Task<bool> UpdateAsync(UpdateStoreLocationDto dto)
    {
        var existingLocation = await _locationRepository.GetByIdAsync(dto.LocationId);
        if (existingLocation is null)
            return false;

        var (latitude, longitude) = await _geocodingService.GeocodeAddressAsync(dto.Address);

        existingLocation.Address = dto.Address;
        existingLocation.Latitude = latitude;
        existingLocation.Longitude = longitude;

        return await _locationRepository.UpdateAsync(existingLocation);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _locationRepository.DeleteAsync(id);
    }
}
