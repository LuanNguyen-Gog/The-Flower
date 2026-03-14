using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IChatRepository
{
    Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(int userId, int page, int pageSize);
    Task<ChatMessage> SaveMessageAsync(ChatMessage message);
}
