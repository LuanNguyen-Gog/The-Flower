using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Service.DTOs.Chat;
using Service.DTOs.Response;
using Service.Services.Interfaces;
using TheFlower.Hubs;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hubContext;
    private const string AdminGroup = "admin";

    public ChatsController(IChatService chatService, IHubContext<ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ─── User endpoints ───────────────────────────────────────────────────────

    /// <summary>
    /// Tải lịch sử tin nhắn của user (phân trang)
    /// GET /api/chats/messages?page=1&pageSize=30
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var messages = await _chatService.GetMessagesAsync(GetUserId(), page, pageSize);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Messages retrieved successfully",
                Data = messages
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Gửi tin nhắn qua REST (fallback khi không dùng SignalR)
    /// POST /api/chats/messages
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto { isSuccess = false, Message = "Invalid input", Data = ModelState });

        try
        {
            var message = await _chatService.SendMessageAsync(GetUserId(), dto);
            
            // Broadcast to SignalR
            var userId = GetUserId();
            await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveMessage", message);
            await _hubContext.Clients.Group(AdminGroup).SendAsync("ReceiveUserMessage", new
            {
                UserId = userId,
                Message = message
            });

            return StatusCode(201, new ResponseDto
            {
                isSuccess = true,
                Message = "Message sent successfully",
                Data = message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    // ─── Admin endpoints ──────────────────────────────────────────────────────

    /// <summary>
    /// Admin: list tất cả conversations (users đã nhắn tin)
    /// GET /api/chats/conversations
    /// </summary>
    [HttpGet("conversations")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var conversations = await _chatService.GetAllConversationsAsync();
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Conversations retrieved successfully",
                Data = conversations
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Admin: lịch sử tin nhắn của 1 user cụ thể
    /// GET /api/chats/messages/{userId}?page=1&pageSize=30
    /// </summary>
    [HttpGet("messages/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesForUser(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        try
        {
            var messages = await _chatService.GetMessagesForUserAsync(userId, page, pageSize);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Messages retrieved successfully",
                Data = messages
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Admin: gửi tin nhắn tới 1 user cụ thể qua REST
    /// POST /api/chats/messages/admin
    /// </summary>
    [HttpPost("messages/admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AdminSendMessage([FromBody] AdminSendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto { isSuccess = false, Message = "Invalid input", Data = ModelState });

        try
        {
            var message = await _chatService.SendAdminMessageAsync(dto.TargetUserId, dto.Message);

            // Broadcast to SignalR
            await _hubContext.Clients.Group($"user-{dto.TargetUserId}").SendAsync("ReceiveMessage", message);
            await _hubContext.Clients.Group(AdminGroup).SendAsync("ReceiveUserMessage", new
            {
                UserId = dto.TargetUserId,
                Message = message
            });

            return StatusCode(201, new ResponseDto
            {
                isSuccess = true,
                Message = "Admin message sent successfully",
                Data = message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/chats/messages
    /// Xóa toàn bộ lịch sử chat của user hiện tại
    /// </summary>
    [HttpDelete("messages")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearMyChat()
    {
        try
        {
            await _chatService.ClearChatAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Chat history cleared successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Admin: xóa toàn bộ lịch sử chat của 1 user cụ thể
    /// DELETE /api/chats/messages/{userId}
    /// </summary>
    [HttpDelete("messages/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearUserChat(Guid userId)
    {
        try
        {
            await _chatService.ClearChatAsync(userId);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = $"Chat history for user {userId} cleared successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }
}
