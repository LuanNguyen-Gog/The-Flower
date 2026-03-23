using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Chat;

public class SendMessageDto
{
    [Required(ErrorMessage = "Message cannot be empty.")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}

public class AdminSendMessageDto
{
    [Required]
    public Guid TargetUserId { get; set; }

    [Required(ErrorMessage = "Message cannot be empty.")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}

public class ChatMessageDto
{
    public Guid ChatMessageId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    /// <summary>
    /// true = tin từ admin, false = tin từ user
    /// </summary>
    public bool IsFromAdmin { get; set; }
}

/// <summary>
/// Dùng cho admin: thông tin 1 cuộc hội thoại
/// </summary>
public class ConversationSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
