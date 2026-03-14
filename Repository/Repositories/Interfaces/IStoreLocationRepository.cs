using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IStoreLocationRepository
{
    Task<IEnumerable<StoreLocation>> GetAllAsync();
    Task<StoreLocation?> GetByIdAsync(int id);
    Task<StoreLocation> CreateAsync(StoreLocation location);
    Task<bool> UpdateAsync(StoreLocation location);
    Task<bool> DeleteAsync(int id);
}
