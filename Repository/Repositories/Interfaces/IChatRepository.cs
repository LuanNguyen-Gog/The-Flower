using Repository.Models;

namespace Repository.Repositories.Interfaces;

public interface IChatRepository
{
    Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(Guid userId, int page, int pageSize);
    Task<ChatMessage> SaveMessageAsync(ChatMessage message);

    /// <summary>
    /// Batch save multiple messages to database
    /// </summary>
    Task BatchSaveAsync(IEnumerable<ChatMessage> messages);

    /// <summary>
    /// Count total messages for a user (used to detect first-time chat)
    /// </summary>
    Task<int> CountMessagesByUserIdAsync(Guid userId);

    /// <summary>
    /// Get all distinct userIds that have sent messages (for admin view)
    /// </summary>
    Task<IEnumerable<Guid>> GetAllSenderUserIdsAsync();
}
