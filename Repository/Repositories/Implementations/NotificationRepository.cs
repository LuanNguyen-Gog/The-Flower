using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class NotificationRepository : INotificationRepository
{
    private readonly SalesAppDBContext _context;

    public NotificationRepository(SalesAppDBContext context) => _context = context;

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        => await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(Guid userId)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<Notification?> GetByIdAsync(Guid id)
        => await _context.Notifications.FindAsync(id);

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification is null) return;
        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task DeleteAsync(Guid notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification is null) return;
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
    }
}
