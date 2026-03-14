using Service.DTOs.Chat;

namespace Service.Services.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int userId, int page, int pageSize);
    Task<ChatMessageDto> SendMessageAsync(int userId, SendMessageDto dto);
}
