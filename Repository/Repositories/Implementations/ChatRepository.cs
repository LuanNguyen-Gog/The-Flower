using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly SalesAppDBContext _context;

    public ChatRepository(SalesAppDBContext context) => _context = context;

    public async Task<IEnumerable<ChatMessage>> GetMessagesByUserIdAsync(int userId, int page, int pageSize)
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
}
