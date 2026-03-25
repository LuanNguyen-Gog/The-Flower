using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly SalesAppDBContext _context;

    public ChatRepository(SalesAppDBContext context) => _context = context;

    public async Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(Guid userId, int page, int pageSize)
        => await _context.ChatMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

    public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task BatchSaveAsync(IEnumerable<ChatMessage> messages)
    {
        var messageList = messages.ToList();
        if (messageList.Count == 0)
            return;

        _context.ChatMessages.AddRange(messageList);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountMessagesByUserIdAsync(Guid userId)
        => await _context.ChatMessages.CountAsync(m => m.UserId == userId);

    public async Task<IEnumerable<Guid>> GetAllSenderUserIdsAsync()
        => await _context.ChatMessages
            .Where(m => !m.IsFromAdmin && m.UserId != null)
            .Select(m => m.UserId!.Value)
            .Distinct()
            .ToListAsync();

    public async Task DeleteMessagesByUserIdAsync(Guid userId)
    {
        var messages = _context.ChatMessages.Where(m => m.UserId == userId);
        _context.ChatMessages.RemoveRange(messages);
        await _context.SaveChangesAsync();
    }
}
