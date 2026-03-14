using Service.DTOs.Chat;

namespace Service.Services.Interfaces;

public interface IChatService
{
    /// <summary>
    /// Get messages for a user (from cache if available, DB if requesting older messages)
    /// </summary>
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int userId, int page, int pageSize);
    
    /// <summary>
    /// Send message - adds to cache immediately (non-blocking), returns cached copy
    /// DB persistence happens asynchronously in background
    /// </summary>
    Task<ChatMessageDto> SendMessageAsync(int userId, SendMessageDto dto);
    /// <summary>
    /// Start the background persistence job (call once on app startup)
    /// Periodically flushes pending messages to database
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop the background persistence job and perform final flush
    /// </summary>
    Task StopAsync();}

