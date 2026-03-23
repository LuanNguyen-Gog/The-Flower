using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(Guid id);
}
