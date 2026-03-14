using Microsoft.AspNetCore.SignalR;
using Service.DTOs.Notifications;
using Service.Services.Interfaces;
using TheFlower.Hubs;

namespace TheFlower.Services;

/// <summary>
/// Implementation of INotificationBroadcaster using SignalR hub
/// </summary>
public class NotificationBroadcaster : INotificationBroadcaster
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationBroadcaster(IHubContext<NotificationHub> hubContext)
        => _hubContext = hubContext;

    public async Task SendNotificationAsync(int userId, NotificationDto notification)
    {
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("ReceiveNotification", notification);
    }
}
