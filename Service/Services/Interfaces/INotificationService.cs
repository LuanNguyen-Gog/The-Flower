using Service.DTOs.Notifications;

namespace Service.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId);
    Task<BadgeDto> GetBadgeAsync(Guid userId);
    Task<NotificationDto> CreateNotificationAsync(Guid userId, string message);
    Task<NotificationDto> SendNotificationAsync(Guid userId, string message);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid userId, Guid notificationId);
}
