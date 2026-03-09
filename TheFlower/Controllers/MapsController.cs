using Microsoft.AspNetCore.Mvc;
using Service.Services;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStores()
        => Ok(await _locationService.GetAllAsync());

    /// <summary>
    /// Lấy chi tiết một cửa hàng theo ID
    /// GET /api/maps/stores/{id}
    /// </summary>
    [HttpGet("stores/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStore(int id)
    {
        var store = await _locationService.GetByIdAsync(id);
        return store is null ? NotFound(new { message = "Store not found." }) : Ok(store);
    }
}
