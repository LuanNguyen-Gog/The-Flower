using Repository.Repositories;
using Service.DTOs.Notifications;

namespace Service.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICartRepository _cartRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        ICartRepository cartRepository)
    {
        _notificationRepository = notificationRepository;
        _cartRepository = cartRepository;
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        return notifications.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            Message = n.Message ?? string.Empty,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<BadgeDto> GetBadgeAsync(int userId)
    {
        var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
        var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);
        var cartItemCount = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;

        return new BadgeDto
        {
            UnreadNotifications = unreadCount,
            CartItemCount = cartItemCount
        };
    }

    public async Task MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _notificationRepository.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadAsync(int userId)
        => await _notificationRepository.MarkAllAsReadAsync(userId);
}
