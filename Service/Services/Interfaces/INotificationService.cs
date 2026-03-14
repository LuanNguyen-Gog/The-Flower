using Service.DTOs.Notifications;

namespace Service.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId);
    Task<BadgeDto> GetBadgeAsync(int userId);
    Task MarkAsReadAsync(int userId, int notificationId);
    Task MarkAllAsReadAsync(int userId);
}
