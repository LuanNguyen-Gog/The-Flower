using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Response;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
        => _notificationService = notificationService;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Lấy tất cả thông báo của user
    /// GET /api/notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Notifications retrieved successfully",
                Data = notifications
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
    /// Lấy số badge: số thông báo chưa đọc + số item trong giỏ hàng
    /// GET /api/notifications/badge
    /// </summary>
    [HttpGet("badge")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBadge()
    {
        try
        {
            var badge = await _notificationService.GetBadgeAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Badge retrieved successfully",
                Data = badge
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
    /// Đánh dấu một thông báo đã đọc
    /// PUT /api/notifications/{id}/read
    /// </summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(GetUserId(), id);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Notification marked as read successfully",
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
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ResponseDto
            {
                isSuccess = false,
                Message = "Access denied",
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
    /// Đánh dấu tất cả thông báo đã đọc
    /// PUT /api/notifications/read-all
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            await _notificationService.MarkAllAsReadAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "All notifications marked as read successfully",
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
