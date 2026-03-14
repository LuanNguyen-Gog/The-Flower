using Service.DTOs.Maps;

namespace Service.Services.Interfaces;

public interface IStoreLocationService
{
    Task<IEnumerable<StoreLocationDto>> GetAllAsync();
    Task<StoreLocationDto?> GetByIdAsync(int id);
}
