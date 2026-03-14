using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Products;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
        => _productService = productService;

    /// <summary>
    /// Lấy danh sách hoa — có phân trang, lọc, sắp xếp
    /// GET /api/products?page=1&pageSize=10&categoryId=1&minPrice=50000&maxPrice=500000&sortBy=price&sortOrder=asc
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto query)
        => Ok(await _productService.GetProductsAsync(query));

    /// <summary>
    /// Xem chi tiết một loại hoa
    /// GET /api/products/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return product is null ? NotFound(new { message = "Product not found." }) : Ok(product);
    }

    /// <summary>
    /// Lấy tất cả danh mục hoa
    /// GET /api/products/categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
        => Ok(await _productService.GetCategoriesAsync());
}
