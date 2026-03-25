using Microsoft.Extensions.DependencyInjection;
using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Chat;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

/// <summary>
/// Chat service with integrated in-memory caching and background persistence.
///
/// Memory leak prevention:
/// - Singleton with CancellationTokenSource properly disposed on StopAsync
/// - Background loop uses Task.Delay with cancellation token
/// - DI scopes created/disposed per operation (no scope leak)
/// - LinkedList cache capped at MaxMessagesPerUser per user
/// </summary>
public class ChatService : IChatService
{
    private readonly IServiceProvider _serviceProvider;

    // ── Welcome messages ────────────────────────────────────────────────────
    private static readonly (string message, TimeSpan delay)[] WelcomeMessages =
    [
        ("Xin chào! 🌸 Chào mừng bạn đến với The Flower! Chúng tôi rất vui được gặp bạn.", TimeSpan.Zero),
        ("Bạn cần tư vấn về hoa hay cần hỗ trợ gì không? Chúng tôi luôn sẵn sàng giúp bạn! 😊", TimeSpan.FromSeconds(1))
    ];

    // ── In-Memory Cache ──────────────────────────────────────────────────────
    private const int MaxMessagesPerUser = 100;
    private const int BatchIntervalMs = 5000;

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
    // ── PUBLIC OPERATIONS ───────────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(Guid userId, int page, int pageSize)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        IEnumerable<ChatMessage> messages;

        if (page == 1)
        {
            var cached = GetCachedMessages(userId);
            messages = cached.Count() < pageSize
                ? await repo.GetMessagesByUserIdAsync(userId, page, pageSize)
                : cached.TakeLast(pageSize);
        }
        else
        {
            messages = await repo.GetMessagesByUserIdAsync(userId, page, pageSize);
        }

        return messages.Select(MapToDto);
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesForUserAsync(Guid targetUserId, int page, int pageSize)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
        var messages = await repo.GetMessagesByUserIdAsync(targetUserId, page, pageSize);
        return messages.Select(MapToDto);
    }

    public async Task<IEnumerable<ConversationSummaryDto>> GetAllConversationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
        var userIds = await repo.GetAllSenderUserIdsAsync();

        var result = new List<ConversationSummaryDto>();
        foreach (var userId in userIds)
        {
            var msgs = await repo.GetMessagesByUserIdAsync(userId, 1, 1);
            var last = msgs.FirstOrDefault();
            result.Add(new ConversationSummaryDto
            {
                UserId = userId,
                LastMessage = last?.Message ?? string.Empty,
                LastMessageAt = last?.SentAt ?? DateTime.MinValue
            });
        }
        return result.OrderByDescending(c => c.LastMessageAt);
    }

    public async Task<bool> HasMessagesAsync(Guid userId)
    {
        // Check cache first (fast path)
        var cached = GetCachedMessages(userId);
        if (cached.Any()) return true;

        // Fallback to DB
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
        return await repo.CountMessagesByUserIdAsync(userId) > 0;
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid userId, SendMessageDto dto)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        var message = new ChatMessage
        {
            UserId = userId,
            Message = dto.Message,
            SentAt = DateTime.UtcNow.AddHours(7),
            Status = "Active",
            IsFromAdmin = false
        };

        AddMessage(userId, message);
        var saved = await repo.SaveMessageAsync(message);
        message.ChatMessageId = saved.ChatMessageId;
        RemoveFromPending(saved.ChatMessageId);

        return MapToDto(message);
    }

    public async Task<ChatMessageDto> SendAdminMessageAsync(Guid targetUserId, string message)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        var chatMessage = new ChatMessage
        {
            UserId = targetUserId,       // stored under the user's conversation
            Message = message,
            SentAt = DateTime.UtcNow.AddHours(7),
            Status = "Active",
            IsFromAdmin = true
        };

        AddMessage(targetUserId, chatMessage);
        var saved = await repo.SaveMessageAsync(chatMessage);
        chatMessage.ChatMessageId = saved.ChatMessageId;
        RemoveFromPending(saved.ChatMessageId);

        return MapToDto(chatMessage);
    }

    public async Task<IEnumerable<ChatMessageDto>> SendWelcomeMessagesAsync(Guid userId)
    {
        var result = new List<ChatMessageDto>();
        foreach (var (msg, delay) in WelcomeMessages)
        {
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);

            var saved = await SendAdminMessageAsync(userId, msg);
            result.Add(saved);
        }
        return result;
    }

    public async Task ClearChatAsync(Guid userId)
    {
        lock (_lockObject)
        {
            // Clear in-memory cache
            _userMessageCache.Remove(userId);

            // Remove from pending messages (those not yet persisted)
            _pendingMessages.RemoveAll(m => m.UserId == userId);
        }

        // Clear from database
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
        await repo.DeleteMessagesByUserIdAsync(userId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── BACKGROUND PERSISTENCE ──────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task StartAsync()
    {
        if (_cancellationTokenSource is not null)
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        _ = PersistenceLoopAsync(_cancellationTokenSource.Token);
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cancellationTokenSource is null)
            return;

        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;

        await FlushPendingMessagesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── PRIVATE CACHE OPERATIONS ─────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    private void AddMessage(Guid userId, ChatMessage message)
    {
        lock (_lockObject)
        {
            if (!_userMessageCache.ContainsKey(userId))
                _userMessageCache[userId] = new LinkedList<ChatMessage>();

            var userMessages = _userMessageCache[userId];
            userMessages.AddLast(message);
            _pendingMessages.Add(message);

            if (userMessages.Count > MaxMessagesPerUser)
            {
                var oldest = userMessages.First;
                if (oldest != null)
                {
                    userMessages.RemoveFirst();
                    _pendingMessages.Remove(oldest.Value);
                }
            }
        }
    }

    private IEnumerable<ChatMessage> GetCachedMessages(Guid userId)
    {
        lock (_lockObject)
        {
            return _userMessageCache.TryGetValue(userId, out var list)
                ? list.ToList()
                : Enumerable.Empty<ChatMessage>();
        }
    }

    private IEnumerable<ChatMessage> GetPendingMessages()
    {
        lock (_lockObject) { return _pendingMessages.ToList(); }
    }

    private void MarkAsPersisted(IEnumerable<Guid> messageIds)
    {
        lock (_lockObject)
        {
            var ids = messageIds.ToHashSet();
            _pendingMessages.RemoveAll(m => ids.Contains(m.ChatMessageId));
        }
    }

    private void RemoveFromPending(Guid messageId)
    {
        lock (_lockObject)
        {
            _pendingMessages.RemoveAll(m => m.ChatMessageId == messageId);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ── PERSISTENCE LOOP ─────────────────────────────────────────────────────
    // ═══════════════════════════════════════════════════════════════════════════

    private async Task PersistenceLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(BatchIntervalMs, cancellationToken);
                    await FlushPendingMessagesAsync();
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ChatService] Error during persistence: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Fatal error in persistence loop: {ex.Message}");
        }
    }

    private async Task FlushPendingMessagesAsync()
    {
        var pending = GetPendingMessages().ToList();
        if (pending.Count == 0) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
            await repo.BatchSaveAsync(pending);
            MarkAsPersisted(pending.Select(m => m.ChatMessageId));
            Console.WriteLine($"[ChatService] Persisted {pending.Count} messages");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Failed to flush messages: {ex.Message}");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ChatMessageDto MapToDto(ChatMessage m) => new()
    {
        ChatMessageId = m.ChatMessageId,
        UserId = m.UserId ?? Guid.Empty,
        Message = m.Message ?? string.Empty,
        SentAt = m.SentAt,
        IsFromAdmin = m.IsFromAdmin
    };
}
