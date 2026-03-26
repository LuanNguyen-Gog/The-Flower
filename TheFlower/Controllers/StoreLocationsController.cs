using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Maps;
using Service.DTOs.Response;
using Service.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreLocationsController : ControllerBase
{
    private readonly IStoreLocationService _locationService;

    public StoreLocationsController(IStoreLocationService locationService)
        => _locationService = locationService;

    /// <summary>
    /// Lấy tọa độ và địa chỉ tất cả cửa hàng
    /// GET /api/StoreLocations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStores()
    {
        try
        {
            var stores = await _locationService.GetAllAsync();
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Stores retrieved successfully",
                Data = stores
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
    /// Lấy chi tiết một cửa hàng theo ID
    /// GET /api/StoreLocations/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStore(Guid id)
    {
        try
        {
            var store = await _locationService.GetByIdAsync(id);
            if (store is null)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Store not found.",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Store retrieved successfully",
                Data = store
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
    /// Tạo cửa hàng mới (Chỉ Admin)
    /// POST /api/StoreLocations
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStore([FromBody] CreateStoreLocationDto dto)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Address))
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Address is required",
                    Data = null
                });

            var result = await _locationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetStore), new { id = result.LocationId }, new ResponseDto
            {
                isSuccess = true,
                Message = "Store created successfully",
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
    /// Cập nhật thông tin cửa hàng (Chỉ Admin)
    /// PUT /api/StoreLocations/{id}
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStore(Guid id, [FromBody] UpdateStoreLocationDto dto)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Address))
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Address is required",
                    Data = null
                });

            if (id != dto.LocationId)
                return BadRequest(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Location ID mismatch",
                    Data = null
                });

            var result = await _locationService.UpdateAsync(dto);
            if (!result)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Store not found",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Store updated successfully",
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
    /// Xóa cửa hàng (Chỉ Admin)
    /// DELETE /api/StoreLocations/{id}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStore(Guid id)
    {
        try
        {
            var result = await _locationService.DeleteAsync(id);
            if (!result)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Store not found",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Store deleted successfully",
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
