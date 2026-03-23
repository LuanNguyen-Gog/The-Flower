using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Chat;
using Service.DTOs.Response;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsController(IChatService chatService) => _chatService = chatService;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Tải lịch sử tin nhắn của user (có phân trang)
    /// GET /api/chats/messages?page=1&pageSize=20
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
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
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    /// <summary>
    /// Gửi tin nhắn qua REST (thay thế khi không dùng SignalR)
    /// POST /api/chats/messages
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message = "Invalid input",
                Data = ModelState
            });

        try
        {
            var message = await _chatService.SendMessageAsync(GetUserId(), dto);
            return StatusCode(StatusCodes.Status201Created, new ResponseDto
            {
                isSuccess = true,
                Message = "Message sent successfully",
                Data = message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }
}
