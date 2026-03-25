using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Response;
using Service.DTOs.Users;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private Guid GetUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");

        return Guid.Parse(claimValue);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();
        return Ok(new ResponseDto
        {
            isSuccess = true,
            Message = "Users retrieved successfully",
            Data = users
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto { isSuccess = false, Message = "Invalid input", Data = ModelState });

        try
        {
            var created = await _userService.CreateUserAsync(dto);
            return StatusCode(StatusCodes.Status201Created, new ResponseDto
            {
                isSuccess = true,
                Message = "User created successfully",
                Data = created
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateAdminUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResponseDto { isSuccess = false, Message = "Invalid input", Data = ModelState });

        try
        {
            var updated = await _userService.UpdateUserAsync(id, dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "User updated successfully",
                Data = updated
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto { isSuccess = false, Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ResponseDto { isSuccess = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        if (GetUserId() == id)
            return BadRequest(new ResponseDto { isSuccess = false, Message = "You cannot delete your own account." });

        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(new ResponseDto { isSuccess = false, Message = "User not found." });

        return Ok(new ResponseDto { isSuccess = true, Message = "User deleted successfully" });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var profile = await _userService.GetUserProfileAsync(GetUserId());
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Profile retrieved successfully",
                Data = profile
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
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
            var profile = await _userService.UpdateUserProfileAsync(GetUserId(), dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Profile updated successfully",
                Data = profile
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpPost("change-password")]
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeUserPasswordDto dto)
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
            await _userService.ChangePasswordAsync(GetUserId(), dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Password changed successfully",
                Data = null
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
            });
        }
    }
}
