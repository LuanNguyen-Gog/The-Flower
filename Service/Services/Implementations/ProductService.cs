using Repository.Repositories.Interfaces;
using Repository.Models;
using Service.DTOs.Common;
using Service.DTOs.Products;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

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
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? string.Empty,
                StockQuantity = p.StockQuantity ?? 0
            }),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(Guid id)
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
            CategoryId = p.CategoryId,
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

    public async Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            ProductName = dto.ProductName,
            BriefDescription = dto.BriefDescription,
            FullDescription = dto.FullDescription,
            TechnicalSpecifications = dto.TechnicalSpecifications,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            StockQuantity = dto.StockQuantity,
            Status = "Active"
        };

        var createdProduct = await _productRepository.CreateAsync(product);

        return new ProductDetailDto
        {
            ProductId = createdProduct.ProductId,
            ProductName = createdProduct.ProductName,
            BriefDescription = createdProduct.BriefDescription,
            FullDescription = createdProduct.FullDescription,
            TechnicalSpecifications = createdProduct.TechnicalSpecifications,
            Price = createdProduct.Price,
            ImageUrl = createdProduct.ImageUrl,
            CategoryId = createdProduct.CategoryId,
            CategoryName = string.Empty,
            StockQuantity = createdProduct.StockQuantity ?? 0
        };
    }

    public async Task<bool> UpdateProductAsync(UpdateProductDto dto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(dto.ProductId);
        if (existingProduct is null)
            return false;

        existingProduct.ProductName = dto.ProductName;
        existingProduct.BriefDescription = dto.BriefDescription;
        existingProduct.FullDescription = dto.FullDescription;
        existingProduct.TechnicalSpecifications = dto.TechnicalSpecifications;
        existingProduct.Price = dto.Price;
        existingProduct.ImageUrl = dto.ImageUrl;
        existingProduct.CategoryId = dto.CategoryId;
        existingProduct.StockQuantity = dto.StockQuantity;

        return await _productRepository.UpdateAsync(existingProduct);
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        return await _productRepository.DeleteAsync(id);
    }
}
