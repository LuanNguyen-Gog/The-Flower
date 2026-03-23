using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Categories;
using Service.DTOs.Response;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
        => _categoryService = categoryService;

    /// <summary>
    /// Lấy tất cả danh mục
    /// GET /api/categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Categories retrieved successfully",
                Data = categories
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
    /// Lấy chi tiết danh mục theo ID
    /// GET /api/categories/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id)
                ?? throw new KeyNotFoundException("Category not found.");

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Category retrieved successfully",
                Data = category
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
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
    /// Tạo danh mục mới
    /// POST /api/categories
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Invalid input",
                    Data = ModelState
                });

            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, new ResponseDto
            {
                isSuccess = true,
                Message = "Category created successfully",
                Data = category
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
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
    /// Cập nhật danh mục
    /// PUT /api/categories/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Invalid input",
                    Data = ModelState
                });

            var success = await _categoryService.UpdateCategoryAsync(id, dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Category updated successfully",
                Data = null
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
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
    /// Xóa danh mục
    /// DELETE /api/categories/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Category deleted successfully",
                Data = null
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
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
