using Repository.Models;

namespace Repository.Repositories;

public interface IChatRepository
{
    Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(int userId, int page, int pageSize);
    Task<ChatMessage> SaveMessageAsync(ChatMessage message);
}
