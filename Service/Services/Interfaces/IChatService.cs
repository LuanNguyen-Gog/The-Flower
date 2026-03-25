using Service.DTOs.Chat;

namespace Service.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Get messages for a user (from cache if available, DB for older messages)
    /// </summary>
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid userId, int page, int pageSize);

    /// <summary>
    /// Admin: get messages for a specific user conversation
    /// </summary>
    Task<IEnumerable<ChatMessageDto>> GetMessagesForUserAsync(Guid targetUserId, int page, int pageSize);

    /// <summary>
    /// Admin: list all users who have sent at least one message
    /// </summary>
    Task<IEnumerable<ConversationSummaryDto>> GetAllConversationsAsync();

    /// <summary>
    /// Check if user has any messages yet (used for welcome message logic)
    /// </summary>
    Task<bool> HasMessagesAsync(Guid userId);

    /// <summary>
    /// Send message from user (IsFromAdmin = false)
    /// </summary>
    Task<ChatMessageDto> SendMessageAsync(Guid userId, SendMessageDto dto);

    /// <summary>
    /// Send message from admin to a specific user (IsFromAdmin = true)
    /// </summary>
    Task<ChatMessageDto> SendAdminMessageAsync(Guid targetUserId, string message);

    /// <summary>
    /// Send welcome messages when user opens chat for first time.
    /// Returns the list of welcome messages saved.
    /// </summary>
    Task<IEnumerable<ChatMessageDto>> SendWelcomeMessagesAsync(Guid userId);

    /// <summary>Start the background persistence job (call once on app startup)</summary>
    Task StartAsync();

    /// <summary>Stop the background persistence job and perform final flush</summary>
    Task StopAsync();

    /// <summary>
    /// Clear chat history for a specific user (cache + DB)
    /// </summary>
    Task ClearChatAsync(Guid userId);
}
