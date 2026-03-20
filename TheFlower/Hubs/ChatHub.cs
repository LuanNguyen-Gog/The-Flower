using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service.DTOs.Chat;
using Service.Services.Interfaces;

namespace TheFlower.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService) => _chatService = chatService;

    /// <summary>
    /// Client gọi khi kết nối — tự động join vào group riêng theo userId
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client gọi để gửi tin nhắn.
    /// Android: hubConnection.InvokeAsync("SendMessage", "Nội dung tin nhắn")
    /// </summary>
    public async Task SendMessage(string message)
    {
        var userId = GetUserId();

        var dto = new SendMessageDto { Message = message };
        var saved = await _chatService.SendMessageAsync(userId, dto);

        // Gửi lại cho chính user đó (group của user)
        await Clients.Group($"user-{userId}")
            .SendAsync("ReceiveMessage", saved);
    }

    private Guid GetUserId() =>
        Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
