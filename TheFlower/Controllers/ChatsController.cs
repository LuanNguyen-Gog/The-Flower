using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Chat;
using Service.Services;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsController(IChatService chatService) => _chatService = chatService;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Tải lịch sử tin nhắn của user (có phân trang)
    /// GET /api/chats/messages?page=1&pageSize=20
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var messages = await _chatService.GetMessagesAsync(GetUserId(), page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Gửi tin nhắn qua REST (thay thế khi không dùng SignalR)
    /// POST /api/chats/messages
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var message = await _chatService.SendMessageAsync(GetUserId(), dto);
        return StatusCode(StatusCodes.Status201Created, message);
    }
}
