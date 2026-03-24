using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// Body: { paymentMethod: "VnPay" | "COD", billingAddress: "..." }
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
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _orderService.CreateOrderAsync(GetUserId(), dto, ipAddress);
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
    /// VnPay Return URL — VnPay redirect browser về đây sau khi thanh toán
    /// GET /api/orders/vnpay-return
    /// ⚠️ Không cần JWT — VnPay tự xác thực bằng HMAC-SHA512 signature
    /// </summary>
    [HttpGet("vnpay-return")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VnPayReturn()
    {
        try
        {
            var queryParams = Request.Query
                .Select(p => new KeyValuePair<string, string>(p.Key, p.Value.ToString()));

            var isSuccess = await _orderService.HandleVnPayReturnAsync(queryParams);
            var orderId = Request.Query["vnp_TxnRef"].ToString();
            var amount = Request.Query["vnp_Amount"].ToString();
            var displayAmount = long.TryParse(amount, out var rawAmount)
                ? $"{rawAmount / 100:N0} VND"
                : "-";
            var title = isSuccess ? "Thanh toan thanh cong" : "Thanh toan that bai";
            var subtitle = isSuccess
                ? "Don hang cua ban da duoc thanh toan thanh cong qua VnPay."
                : "Giao dich khong thanh cong. Vui long thu lai hoac chon phuong thuc khac.";
            var statusColor = isSuccess ? "#16a34a" : "#dc2626";
            var cardBorder = isSuccess ? "#bbf7d0" : "#fecaca";
            var bgTint = isSuccess ? "#f0fdf4" : "#fef2f2";

            var html = $@"
<!doctype html>
<html lang='vi'>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1' />
  <title>{title}</title>
  <style>
    * {{ box-sizing: border-box; }}
    body {{
      margin: 0;
      font-family: 'Segoe UI', Arial, sans-serif;
      background: linear-gradient(135deg, #0f172a, #1e293b);
      color: #0f172a;
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 20px;
    }}
    .card {{
      width: 100%;
      max-width: 560px;
      background: {bgTint};
      border: 1px solid {cardBorder};
      border-radius: 16px;
      padding: 24px;
      box-shadow: 0 20px 60px rgba(0,0,0,.35);
    }}
    .badge {{
      display: inline-block;
      padding: 6px 12px;
      border-radius: 999px;
      font-size: 13px;
      font-weight: 700;
      color: white;
      background: {statusColor};
      margin-bottom: 14px;
    }}
    h1 {{ margin: 0 0 8px; font-size: 28px; color: {statusColor}; }}
    p {{ margin: 0 0 18px; color: #334155; line-height: 1.5; }}
    .info {{
      background: #fff;
      border-radius: 12px;
      padding: 14px;
      border: 1px solid #e2e8f0;
    }}
    .row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px dashed #e2e8f0; }}
    .row:last-child {{ border-bottom: none; }}
    .label {{ color: #64748b; }}
    .value {{ font-weight: 700; color: #0f172a; }}
    .actions {{ margin-top: 20px; display: flex; gap: 10px; flex-wrap: wrap; }}
    .btn {{
      text-decoration: none;
      border: none;
      padding: 10px 14px;
      border-radius: 10px;
      font-weight: 600;
      cursor: pointer;
    }}
    .btn-primary {{ background: #0f172a; color: #fff; }}
    .btn-light {{ background: #e2e8f0; color: #0f172a; }}
  </style>
</head>
<body>
  <div class='card'>
    <span class='badge'>{(isSuccess ? "SUCCESS" : "FAILED")}</span>
    <h1>{title}</h1>
    <p>{subtitle}</p>
    <div class='info'>
      <div class='row'><span class='label'>Ma don hang</span><span class='value'>{orderId}</span></div>
      <div class='row'><span class='label'>So tien</span><span class='value'>{displayAmount}</span></div>
      <div class='row'><span class='label'>Kenh thanh toan</span><span class='value'>VnPay</span></div>
    </div>
    <div class='actions'>
      <a class='btn btn-primary' href='/swagger/index.html'>Ve trang API</a>
      <a class='btn btn-light' href='/api/orders'>Xem danh sach don</a>
    </div>
  </div>
</body>
</html>";

            return Content(html, "text/html; charset=utf-8");
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message   = ex.Message,
                Data      = null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                isSuccess = false,
                Message   = ex.Message,
                Data      = null
            });
        }
    }
}
