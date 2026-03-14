using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IStoreLocationRepository
{
    Task<IEnumerable<StoreLocation>> GetAllAsync();
    Task<StoreLocation?> GetByIdAsync(int id);
}
