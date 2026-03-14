using Service.DTOs.Common;
using Service.DTOs.Products;

namespace Service.Services.Interfaces;

public interface IProductService
{
    Task<PagedResultDto<ProductSummaryDto>> GetProductsAsync(ProductQueryDto query);
    Task<ProductDetailDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
}
