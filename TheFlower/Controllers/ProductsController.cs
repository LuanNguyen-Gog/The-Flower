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
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
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

    /// <summary>
    /// Tạo sản phẩm mới
    /// POST /api/products
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Invalid product data",
                    Data = null
                });

            var result = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, new ResponseDto
            {
                isSuccess = true,
                Message = "Product created successfully",
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
    /// Cập nhật thông tin sản phẩm
    /// PUT /api/products/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Invalid product data",
                    Data = null
                });

            if (id != dto.ProductId)
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Product ID mismatch",
                    Data = null
                });

            var result = await _productService.UpdateProductAsync(dto);
            if (!result)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Product not found",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Product updated successfully",
                Data = null
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
    /// Xóa sản phẩm (soft delete - đặt status thành InActive)
    /// DELETE /api/products/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Product not found",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Product deleted successfully",
                Data = null
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
