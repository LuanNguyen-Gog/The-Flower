using Service.DTOs.Notifications;

namespace Service.Services.Interfaces;

/// <summary>
/// Interface for sending notifications via SignalR (real-time)
/// Implementation is in TheFlower layer
/// </summary>
public interface INotificationBroadcaster
{
    Task SendNotificationAsync(Guid userId, NotificationDto notification);
}
