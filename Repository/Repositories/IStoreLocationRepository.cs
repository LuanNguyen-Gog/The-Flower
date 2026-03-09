using Repository.Models;

namespace Repository.Repositories;

public interface IStoreLocationRepository
{
    Task<IEnumerable<StoreLocation>> GetAllAsync();
    Task<StoreLocation?> GetByIdAsync(int id);
}
