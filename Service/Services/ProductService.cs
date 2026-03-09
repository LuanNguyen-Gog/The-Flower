using Repository.Repositories;
using Service.DTOs.Common;
using Service.DTOs.Products;

namespace Service.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
        => _productRepository = productRepository;

    public async Task<PagedResultDto<ProductSummaryDto>> GetProductsAsync(ProductQueryDto query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var (items, totalCount) = await _productRepository.GetPagedAsync(
            page, pageSize, query.CategoryId,
            query.MinPrice, query.MaxPrice,
            query.SortBy, query.SortOrder);

        return new PagedResultDto<ProductSummaryDto>
        {
            Items = items.Select(p => new ProductSummaryDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                BriefDescription = p.BriefDescription,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId ?? 0,
                CategoryName = p.Category?.CategoryName ?? string.Empty,
                StockQuantity = p.StockQuantity ?? 0
            }),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
    {
        var p = await _productRepository.GetByIdAsync(id);
        if (p is null) return null;

        return new ProductDetailDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            BriefDescription = p.BriefDescription,
            FullDescription = p.FullDescription,
            TechnicalSpecifications = p.TechnicalSpecifications,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId ?? 0,
            CategoryName = p.Category?.CategoryName ?? string.Empty,
            StockQuantity = p.StockQuantity ?? 0
        };
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _productRepository.GetAllCategoriesAsync();
        return categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName
        });
    }
}
