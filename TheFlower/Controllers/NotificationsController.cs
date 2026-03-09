using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications()
        => Ok(await _notificationService.GetNotificationsAsync(GetUserId()));

    /// <summary>
    /// Lấy số badge: số thông báo chưa đọc + số item trong giỏ hàng
    /// GET /api/notifications/badge
    /// </summary>
    [HttpGet("badge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBadge()
        => Ok(await _notificationService.GetBadgeAsync(GetUserId()));

    /// <summary>
    /// Đánh dấu một thông báo đã đọc
    /// PUT /api/notifications/{id}/read
    /// </summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(GetUserId(), id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo đã đọc
    /// PUT /api/notifications/read-all
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(GetUserId());
        return NoContent();
    }
}
