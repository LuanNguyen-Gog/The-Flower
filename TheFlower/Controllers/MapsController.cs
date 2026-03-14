using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Maps;
using Service.DTOs.Response;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MapsController : ControllerBase
{
    private readonly IStoreLocationService _locationService;

    public MapsController(IStoreLocationService locationService)
        => _locationService = locationService;

    /// <summary>
    /// Lấy tọa độ và địa chỉ tất cả cửa hàng
    /// GET /api/maps/stores
    /// </summary>
    [HttpGet("stores")]
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
    /// GET /api/maps/stores/{id}
    /// </summary>
    [HttpGet("stores/{id:int}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStore(int id)
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
    /// Tạo cửa hàng mới - Backend tự động geocode địa chỉ thành lat/long
    /// POST /api/maps/stores
    /// </summary>
    [HttpPost("stores")]
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
            return CreatedAtAction("GetStore", new { id = result.LocationId }, new ResponseDto
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
    /// Cập nhật thông tin cửa hàng - Backend sẽ geocode địa chỉ mới
    /// PUT /api/maps/stores/{id}
    /// </summary>
    [HttpPut("stores/{id:int}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStore(int id, [FromBody] UpdateStoreLocationDto dto)
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
    /// Xóa cửa hàng (soft delete - đặt status thành InActive)
    /// DELETE /api/maps/stores/{id}
    /// </summary>
    [HttpDelete("stores/{id:int}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStore(int id)
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
