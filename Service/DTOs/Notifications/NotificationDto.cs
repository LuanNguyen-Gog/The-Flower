namespace Service.DTOs.Notifications;

public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BadgeDto
{
    public int UnreadNotifications { get; set; }
    public int CartItemCount { get; set; }
}
