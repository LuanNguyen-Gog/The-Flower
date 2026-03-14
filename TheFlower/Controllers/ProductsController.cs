using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Products;
using Service.DTOs.Response;
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
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto query)
    {
        try
        {
            var result = await _productService.GetProductsAsync(query);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Products retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    /// <summary>
    /// Xem chi tiết một loại hoa
    /// GET /api/products/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product is null)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Product not found",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Product retrieved successfully",
                Data = product
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    /// <summary>
    /// Lấy tất cả danh mục hoa
    /// GET /api/products/categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var result = await _productService.GetCategoriesAsync();
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Categories retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }
}
