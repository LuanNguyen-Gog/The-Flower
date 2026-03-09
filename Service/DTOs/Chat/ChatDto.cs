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
    public int ChatMessageId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
