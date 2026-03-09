using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Cart;
using Service.Services;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService) => _cartService = cartService;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Lấy giỏ hàng hiện tại của user (tạo mới nếu chưa có)
    /// GET /api/carts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart()
        => Ok(await _cartService.GetCartAsync(GetUserId()));

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng
    /// POST /api/carts/items
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var cart = await _cartService.AddItemAsync(GetUserId(), dto);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Cập nhật số lượng một item trong giỏ hàng
    /// PUT /api/carts/items/{cartItemId}
    /// </summary>
    [HttpPut("items/{cartItemId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var cart = await _cartService.UpdateItemQuantityAsync(GetUserId(), cartItemId, dto);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    /// <summary>
    /// Xóa một item khỏi giỏ hàng
    /// DELETE /api/carts/items/{cartItemId}
    /// </summary>
    [HttpDelete("items/{cartItemId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        try
        {
            var cart = await _cartService.RemoveItemAsync(GetUserId(), cartItemId);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    /// <summary>
    /// Xóa toàn bộ giỏ hàng
    /// DELETE /api/carts
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(GetUserId());
        return NoContent();
    }
}
