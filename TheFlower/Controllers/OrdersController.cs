using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using Service.DTOs.Orders;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Tạo đơn hàng từ giỏ hàng hiện tại
    /// POST /api/orders
    /// Body: { paymentMethod: "PayOS" | "COD", billingAddress: "...", returnUrl: "...", cancelUrl: "..." }
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateOrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _orderService.CreateOrderAsync(GetUserId(), dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>
    /// Lấy lịch sử đơn hàng của user
    /// GET /api/orders
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders()
        => Ok(await _orderService.GetOrdersByUserIdAsync(GetUserId()));

    /// <summary>
    /// Xem chi tiết đơn hàng
    /// GET /api/orders/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(GetUserId(), id);
        return order is null ? NotFound(new { message = "Order not found." }) : Ok(order);
    }

    /// <summary>
    /// PayOS Webhook — PayOS gọi endpoint này sau khi thanh toán
    /// POST /api/orders/payos-webhook
    /// ⚠️ Không cần JWT — PayOS tự xác thực bằng checksum signature
    /// </summary>
    [HttpPost("payos-webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayOsWebhook([FromBody] Webhook webhookBody)
    {
        try
        {
            await _orderService.HandlePayOsWebhookAsync(webhookBody);
            return Ok(new { message = "Webhook processed successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
