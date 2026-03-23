using System.ComponentModel.DataAnnotations;

namespace Service.DTOs.Chat;

public class SendMessageDto
{
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
}
