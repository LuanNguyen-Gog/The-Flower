using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification> CreateAsync(Notification notification);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid notificationId);
}
