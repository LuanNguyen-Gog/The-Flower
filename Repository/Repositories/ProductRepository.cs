using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly SalesAppDBContext _context;

    public ProductRepository(SalesAppDBContext context) => _context = context;

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, int? categoryId,
        decimal? minPrice, decimal? maxPrice,
        string? sortBy, string? sortOrder)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync();

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("price", "desc") => query.OrderByDescending(p => p.Price),
            ("price", _) => query.OrderBy(p => p.Price),
            ("name", "desc") => query.OrderByDescending(p => p.ProductName),
            ("name", _) => query.OrderBy(p => p.ProductName),
            _ => query.OrderBy(p => p.ProductId)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        => await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
}
