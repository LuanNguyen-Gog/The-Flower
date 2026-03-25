using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice,
        string? sortBy, string? sortOrder);

    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
}
