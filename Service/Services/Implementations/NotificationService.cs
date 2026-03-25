using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Notifications;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICartRepository _cartRepository;
    private readonly INotificationBroadcaster? _broadcaster;

    public NotificationService(
        INotificationRepository notificationRepository,
        ICartRepository cartRepository,
        INotificationBroadcaster? broadcaster = null)
    {
        _notificationRepository = notificationRepository;
        _cartRepository = cartRepository;
        _broadcaster = broadcaster;
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId)
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

    public async Task<BadgeDto> GetBadgeAsync(Guid userId)
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

    public async Task<NotificationDto> CreateNotificationAsync(Guid userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        var createdNotification = await _notificationRepository.CreateAsync(notification);
        var notificationDto = new NotificationDto
        {
            NotificationId = createdNotification.NotificationId,
            Message = createdNotification.Message ?? string.Empty,
            IsRead = createdNotification.IsRead,
            CreatedAt = createdNotification.CreatedAt
        };

        // Gửi real-time qua SignalR (nếu broadcaster được inject)
        if (_broadcaster is not null)
        {
            try
            {
                await _broadcaster.SendNotificationAsync(userId, notificationDto);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - still want to save notification to DB
                Console.WriteLine($"Failed to broadcast notification: {ex.Message}");
            }
        }

        return notificationDto;
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _notificationRepository.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
        => await _notificationRepository.MarkAllAsReadAsync(userId);

    public async Task DeleteNotificationAsync(Guid userId, Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        await _notificationRepository.DeleteAsync(notificationId);
    }

    /// <summary>
    /// Gửi thông báo - lưu vào DB + broadcast real-time qua SignalR
    /// </summary>
    public async Task<NotificationDto> SendNotificationAsync(Guid userId, string message)
    {
        // Tạo và lưu notification
        var notificationDto = await CreateNotificationAsync(userId, message);

        // Broadcast real-time (logic của INotificationBroadcaster)
        if (_broadcaster is not null)
        {
            try
            {
                await _broadcaster.SendNotificationAsync(userId, notificationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to broadcast notification: {ex.Message}");
            }
        }

        return notificationDto;
    }

    private static NotificationDto MapToDto(Notification notification) => new()
    {
        NotificationId = notification.NotificationId,
        Message = notification.Message ?? string.Empty,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };
}
