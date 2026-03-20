using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service.DTOs.Notifications;

namespace TheFlower.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Client kết nối - tự động join vào group riêng theo userId
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Server gọi client: ReceiveNotification
    /// Android: hubConnection.On<NotificationDto>("ReceiveNotification", notification => { ... })
    /// </summary>
    /// 
    private Guid GetUserId() =>
        Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
