using Service.DTOs.Maps;

namespace Service.Services.Interfaces;

public interface IStoreLocationService
{
    Task<IEnumerable<StoreLocationDto>> GetAllAsync();
    Task<StoreLocationDto?> GetByIdAsync(int id);
    Task<StoreLocationDto> CreateAsync(CreateStoreLocationDto dto);
    Task<bool> UpdateAsync(UpdateStoreLocationDto dto);
    Task<bool> DeleteAsync(int id);
}
