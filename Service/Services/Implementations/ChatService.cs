using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Chat;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository) => _chatRepository = chatRepository;

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int userId, int page, int pageSize)
    {
        var messages = await _chatRepository.GetMessagesByUserIdAsync(userId, page, pageSize);
        return messages.Select(MapToDto);
    }

    public async Task<ChatMessageDto> SendMessageAsync(int userId, SendMessageDto dto)
    {
        var message = await _chatRepository.SaveMessageAsync(new ChatMessage
        {
            UserId = userId,
            Message = dto.Message,
            SentAt = DateTime.UtcNow
        });

        return MapToDto(message);
    }

    private static ChatMessageDto MapToDto(ChatMessage m) => new()
    {
        ChatMessageId = m.ChatMessageId,
        UserId = m.UserId ?? 0,
        Message = m.Message ?? string.Empty,
        SentAt = m.SentAt
    };
}
