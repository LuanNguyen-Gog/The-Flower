using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service.DTOs.Chat;
using Service.Services.Interfaces;

namespace TheFlower.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private const string AdminGroup = "admin";
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService) => _chatService = chatService;

    // ── Connection lifecycle ───────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var role = GetRole();

        // Each user joins their own private group; admin joins the shared admin group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        if (IsAdmin(role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }
        else
        {
            // Send welcome messages if first-time chat
            var hasMessages = await _chatService.HasMessagesAsync(userId);
            if (!hasMessages)
            {
                var welcomes = await _chatService.SendWelcomeMessagesAsync(userId);
                foreach (var welcome in welcomes)
                {
                    // Push directly to this connection (user hasn't joined group yet before base call)
                    await Clients.Caller.SendAsync("ReceiveMessage", welcome);
                }
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var role = GetRole();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        if (IsAdmin(role))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);

        await base.OnDisconnectedAsync(exception);
    }

    // ── User → Admin ───────────────────────────────────────────────────────

    /// <summary>
    /// Called by user client.
    /// Android: hubConnection.InvokeAsync("SendMessage", "Nội dung")
    /// </summary>
    public async Task SendMessage(string message)
    {
        var userId = GetUserId();
        var dto = new SendMessageDto { Message = message };
        var saved = await _chatService.SendMessageAsync(userId, dto);

        // Echo back to sender's own group (all devices of the same user)
        await Clients.Group($"user-{userId}").SendAsync("ReceiveMessage", saved);

        // Broadcast to all connected admins
        await Clients.Group(AdminGroup).SendAsync("ReceiveUserMessage", new
        {
            UserId = userId,
            Message = saved
        });
    }

    // ── Admin → User ───────────────────────────────────────────────────────

    /// <summary>
    /// Called by admin client to reply to a specific user.
    /// Android admin: hubConnection.InvokeAsync("SendMessageToUser", "userId-guid", "Nội dung")
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task SendMessageToUser(string targetUserId, string message)
    {
        if (!Guid.TryParse(targetUserId, out var targetGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid userId format.");
            return;
        }

        var saved = await _chatService.SendAdminMessageAsync(targetGuid, message);

        // Push to the target user's private group
        await Clients.Group($"user-{targetGuid}").SendAsync("ReceiveMessage", saved);

        // Also broadcast to admin group so all admin tabs stay in sync
        await Clients.Group(AdminGroup).SendAsync("ReceiveUserMessage", new
        {
            UserId = targetGuid,
            Message = saved
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private Guid GetUserId() =>
        Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetRole() =>
        Context.User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    private static bool IsAdmin(string role) =>
        role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}
