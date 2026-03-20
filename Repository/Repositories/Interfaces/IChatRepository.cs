using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IChatRepository
{
    Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(Guid userId, int page, int pageSize);
    Task<ChatMessage> SaveMessageAsync(ChatMessage message);
    
    /// <summary>
    /// Batch save multiple messages to database
    /// Used by cache persistence service for efficient bulk insertion
    /// </summary>
    Task BatchSaveAsync(IEnumerable<ChatMessage> messages);
}
