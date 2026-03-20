using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using Service.DTOs.Orders;
using Service.DTOs.Response;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Tạo đơn hàng từ giỏ hàng hiện tại
    /// POST /api/orders
    /// Body: { paymentMethod: "PayOS" | "COD", billingAddress: "...", returnUrl: "...", cancelUrl: "..." }
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
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
            var result = await _orderService.CreateOrderAsync(GetUserId(), dto);
            return StatusCode(StatusCodes.Status201Created, new ResponseDto
            {
                isSuccess = true,
                Message = "Order created successfully",
                Data = result
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
    /// Lấy lịch sử đơn hàng của user
    /// GET /api/orders
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Orders retrieved successfully",
                Data = orders
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
    /// Lấy đơn hàng của một user cụ thể (sắp xếp từ mới nhất)
    /// GET /api/orders/user/{userId}
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByUserId(Guid userId)
    {
        try
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Orders retrieved successfully",
                Data = orders
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
    /// Xem chi tiết đơn hàng
    /// GET /api/orders/{id}
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(GetUserId(), id);
            if (order is null)
                return NotFound(new ResponseDto
                {
                    isSuccess = false,
                    Message = "Order not found",
                    Data = null
                });

            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Order retrieved successfully",
                Data = order
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
    /// PayOS Webhook — PayOS gọi endpoint này sau khi thanh toán
    /// POST /api/orders/payos-webhook
    /// ⚠️ Không cần JWT — PayOS tự xác thực bằng checksum signature
    /// </summary>
    [HttpPost("payos-webhook")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayOsWebhook([FromBody] Webhook webhookBody)
    {
        try
        {
            await _orderService.HandlePayOsWebhookAsync(webhookBody);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Webhook processed successfully",
                Data = null
            });
        }
        catch (UnauthorizedAccessException ex)
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
}
