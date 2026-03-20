using Microsoft.Extensions.DependencyInjection;
using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Chat;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

/// <summary>
/// Chat service with integrated in-memory caching and background persistence
/// 
/// Features:
/// - In-memory cache: Stores 100 most recent messages per user
/// - Batch persistence: Saves pending messages to DB every 5 seconds
/// - Thread-safe operations with lock protection
/// 
/// Flow:
/// 1. SendMessage: Add to cache → Save to DB → Return with ID
/// 2. GetMessages: Return from cache (100 recent) or query DB for older messages
/// 3. Background: Every 5s, batch save pending messages
/// </summary>
public class ChatService : IChatService
{
    private readonly IServiceProvider _serviceProvider;

    // ── In-Memory Cache ──────────────────────────────────────────────────────
    private const int MaxMessagesPerUser = 100;
    private const int BatchIntervalMs = 5000; // 5 seconds

    private readonly Dictionary<Guid, LinkedList<ChatMessage>> _userMessageCache = new();
    private readonly List<ChatMessage> _pendingMessages = new();
    private readonly object _lockObject = new();

    // ── Background Persistence ──────────────────────────────────────────────
    private CancellationTokenSource? _cancellationTokenSource;

    public ChatService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── PUBLIC CHAT OPERATIONS ──────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get messages for a user
    /// Page 1: Try cache first (most recent 100 messages)
    /// Page 2+: Query database for older messages
    /// </summary>
    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid userId, int page, int pageSize)
    {
        // Create a scope to get a fresh DbContext
        using var scope = _serviceProvider.CreateScope();
        var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        IEnumerable<ChatMessage> messages;

        if (page == 1)
        {
            // First page: get from cache (most efficient)
            var cached = GetCachedMessages(userId);

            // If cache has less than pageSize messages, also query DB for older
            if (cached.Count() < pageSize)
            {
                // Get from DB (includes messages not yet in cache)
                messages = await chatRepository.GetMessagesByUserIdAsync(userId, page, pageSize);
            }
            else
            {
                // Enough in cache, take only pageSize
                messages = cached.TakeLast(pageSize);
            }
        }
        else
        {
            // Pagination page 2+: query database for older messages
            messages = await chatRepository.GetMessagesByUserIdAsync(userId, page, pageSize);
        }

        return messages.Select(MapToDto);
    }

    /// <summary>
    /// Send message flow:
    /// 1. Create message object
    /// 2. Add to in-memory cache immediately (for broadcasting to other instances)
    /// 3. Save to database (get assigned ID)
    /// 4. Return to client with valid ID
    /// 
    /// Response time: ~50-100ms (includes DB save) for consistency,
    /// but client gets instant cache retrieval on next GetMessages call
    /// </summary>
    public async Task<ChatMessageDto> SendMessageAsync(Guid userId, SendMessageDto dto)
    {
        // Create a scope to get a fresh DbContext
        using var scope = _serviceProvider.CreateScope();
        var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        var message = new ChatMessage
        {
            UserId = userId,
            Message = dto.Message,
            SentAt = DateTime.UtcNow,
            Status = "Active"
        };

        // Add to in-memory cache for fast retrieval and broadcasting
        AddMessage(userId, message);

        // Save to database (this gets the assigned ChatMessageId)
        var savedMessage = await chatRepository.SaveMessageAsync(message);

        // Update the message object with the real ID from DB
        message.ChatMessageId = savedMessage.ChatMessageId;

        // Important: Remove from pending since we saved immediately
        RemoveFromPending(savedMessage.ChatMessageId);

        return MapToDto(message);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── BACKGROUND PERSISTENCE ──────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start the background persistence job (call once on app startup)
    /// </summary>
    public async Task StartAsync()
    {
        if (_cancellationTokenSource is not null)
            return; // Already running

        _cancellationTokenSource = new CancellationTokenSource();

        // Fire and forget - run in background
        _ = PersistenceLoopAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stop the background persistence job and perform final flush
    /// </summary>
    public async Task StopAsync()
    {
        if (_cancellationTokenSource is null)
            return; // Not running

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;

        // Final flush before stopping
        await FlushPendingMessagesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── PRIVATE CACHE OPERATIONS ─────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Add message to cache (threadsafe)
    /// Automatically tracks in pending list and manages capacity
    /// </summary>
    private void AddMessage(Guid userId, ChatMessage message)
    {
        lock (_lockObject)
        {
            // Ensure cache exists for this user
            if (!_userMessageCache.ContainsKey(userId))
            {
                _userMessageCache[userId] = new LinkedList<ChatMessage>();
            }

            var userMessages = _userMessageCache[userId];

            // Add to cache
            userMessages.AddLast(message);

            // Track as pending for batch persistence
            _pendingMessages.Add(message);

            // Trim if exceeds max (remove oldest - first node)
            if (userMessages.Count > MaxMessagesPerUser)
            {
                var oldestNode = userMessages.First;
                if (oldestNode != null)
                {
                    userMessages.RemoveFirst();

                    // Also remove from pending if not yet saved
                    _pendingMessages.Remove(oldestNode.Value);
                }
            }
        }
    }

    /// <summary>
    /// Get all cached messages for a user (threadsafe)
    /// </summary>
    private IEnumerable<ChatMessage> GetCachedMessages(Guid userId)
    {
        lock (_lockObject)
        {
            if (!_userMessageCache.ContainsKey(userId))
                return Enumerable.Empty<ChatMessage>();

            // Return in chronological order (oldest to newest)
            return _userMessageCache[userId].ToList();
        }
    }

    /// <summary>
    /// Get pending messages to be saved to database (threadsafe)
    /// </summary>
    private IEnumerable<ChatMessage> GetPendingMessages()
    {
        lock (_lockObject)
        {
            return _pendingMessages.ToList();
        }
    }

    /// <summary>
    /// Mark messages as persisted and remove from pending list (threadsafe)
    /// Called after batch save to database
    /// </summary>
    private void MarkAsPersisted(IEnumerable<Guid> messageIds)
    {
        lock (_lockObject)
        {
            var idsToRemove = messageIds.ToHashSet();

            // Remove persisted messages from pending list
            _pendingMessages.RemoveAll(m => idsToRemove.Contains(m.ChatMessageId));
        }
    }

    /// <summary>
    /// Remove a single message from pending list (threadsafe)
    /// Used when a message is saved immediately, not via batch persistence
    /// </summary>
    private void RemoveFromPending(Guid messageId)
    {
        lock (_lockObject)
        {
            _pendingMessages.RemoveAll(m => m.ChatMessageId == messageId);
        }
    }

    /// <summary>
    /// Clear all cache and pending messages (threadsafe)
    /// </summary>
    private void Clear()
    {
        lock (_lockObject)
        {
            _userMessageCache.Clear();
            _pendingMessages.Clear();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── BACKGROUND PERSISTENCE LOOP ──────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Main background loop - runs indefinitely until cancelled
    /// Flushes pending messages every 5 seconds
    /// </summary>
    private async Task PersistenceLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for next batch interval
                    await Task.Delay(BatchIntervalMs, cancellationToken);

                    // Flush pending messages to database
                    await FlushPendingMessagesAsync();
                }
                catch (OperationCanceledException)
                {
                    // Expected when service is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    // Log error but continue looping
                    Console.WriteLine($"[ChatService] Error during persistence: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Fatal error in persistence loop: {ex.Message}");
        }
    }

    /// <summary>
    /// Batch save all pending messages to database
    /// Creates a new DI scope for DbContext (scoped services)
    /// </summary>
    private async Task FlushPendingMessagesAsync()
    {
        var pendingMessages = GetPendingMessages().ToList();

        if (pendingMessages.Count == 0)
            return; // Nothing to save

        try
        {
            // Create a new scope to get a fresh DbContext
            // This is required because repositories are Scoped
            using var scope = _serviceProvider.CreateScope();
            var chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();

            // Batch save to database
            await chatRepository.BatchSaveAsync(pendingMessages);

            // Mark these messages as persisted (remove from pending list)
            var persistedIds = pendingMessages.Select(m => m.ChatMessageId).ToList();
            MarkAsPersisted(persistedIds);

            Console.WriteLine($"[ChatService] Persisted {pendingMessages.Count} messages");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Failed to flush messages: {ex.Message}");
            // Messages remain in pending list, will retry next interval
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── HELPERS ──────────────────────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    private static ChatMessageDto MapToDto(ChatMessage m) => new()
    {
        ChatMessageId = m.ChatMessageId,
        UserId = m.UserId ?? Guid.Empty,
        Message = m.Message ?? string.Empty,
        SentAt = m.SentAt
    };
}

