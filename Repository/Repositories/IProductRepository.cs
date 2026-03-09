using Repository.Models;

namespace Repository.Repositories;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, int? categoryId,
        decimal? minPrice, decimal? maxPrice,
        string? sortBy, string? sortOrder);

    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
}
