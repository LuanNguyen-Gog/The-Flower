using Service.DTOs.Notifications;

namespace Service.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId);
    Task<BadgeDto> GetBadgeAsync(int userId);
    Task<NotificationDto> CreateNotificationAsync(int userId, string message);
    Task<NotificationDto> SendNotificationAsync(int userId, string message);
    Task MarkAsReadAsync(int userId, int notificationId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteNotificationAsync(int userId, int notificationId);
}
