using Microsoft.AspNetCore.Mvc;
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
}
