using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Cart;
using Service.DTOs.Response;
using Service.Services.Interfaces;

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
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var cart = await _cartService.GetCartAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Cart retrieved successfully",
                Data = cart
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
    /// Thêm sản phẩm vào giỏ hàng
    /// POST /api/carts/items
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message = "Invalid input",
                Data = ModelState
            });

        try
        {
            var cart = await _cartService.AddItemAsync(GetUserId(), dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Item added to cart successfully",
                Data = cart
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
    /// Cập nhật số lượng một item trong giỏ hàng
    /// PUT /api/carts/items/{cartItemId}
    /// </summary>
    [HttpPut("items/{cartItemId:int}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message = "Invalid input",
                Data = ModelState
            });

        try
        {
            var cart = await _cartService.UpdateItemQuantityAsync(GetUserId(), cartItemId, dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Item quantity updated successfully",
                Data = cart
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
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
    /// Xóa một item khỏi giỏ hàng
    /// DELETE /api/carts/items/{cartItemId}
    /// </summary>
    [HttpDelete("items/{cartItemId:int}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        try
        {
            var cart = await _cartService.RemoveItemAsync(GetUserId(), cartItemId);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Item removed from cart successfully",
                Data = cart
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
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
    /// Xóa toàn bộ giỏ hàng
    /// DELETE /api/carts
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            await _cartService.ClearCartAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Cart cleared successfully",
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
