using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs.Auth;
using Service.DTOs.Response;
using Service.Services.Interfaces;

namespace TheFlower.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Step 1: Đăng ký tài khoản mới và gửi OTP về email
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
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
            var result = await _authService.RegisterAsync(dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = result.Message,
                Data = new { expiresInMinutes = result.ExpiresInMinutes }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
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
    /// Step 2: Xác minh OTP và hoàn thành đăng ký
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRegisterDto dto)
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
            var response = await _authService.VerifyOtpAndRegisterAsync(dto.Email!, dto.OtpCode!, new RegisterDto
            {
                Email = dto.Email,
                Username = dto.Username,
                Password = dto.Password,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address
            });

            return StatusCode(StatusCodes.Status201Created, new ResponseDto
            {
                isSuccess = true,
                Message = "Email verified successfully. Registration completed.",
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
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
    /// Đăng nhập và nhận JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
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
            var response = await _authService.LoginAsync(dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Login successful",
                Data = response
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
    /// Đổi mật khẩu (cần đăng nhập)
    /// POST /api/auth/change-password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
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
            var response = await _authService.ChangePasswordAsync(GetUserId(), dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Password changed successfully",
                Data = response
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
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
    /// Bước 1: Yêu cầu reset mật khẩu (gửi OTP về email)
    /// POST /api/auth/forgot-password
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
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
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = result.Message,
                Data = new { expiresInMinutes = result.ExpiresInMinutes }
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
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
    /// Bước 2: Xác minh OTP để reset mật khẩu
    /// POST /api/auth/verify-forgot-password-otp
    /// </summary>
    [HttpPost("verify-forgot-password-otp")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpDto dto)
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
            var result = await _authService.VerifyForgotPasswordOtpAsync(dto.Email, dto.OtpCode);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = result.Message,
                Data = new { expiresInMinutes = result.ExpiresInMinutes }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
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
    /// Bước 3: Nhập mật khẩu mới sau khi xác minh OTP
    /// POST /api/auth/reset-password
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
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
            var response = await _authService.ResetPasswordAsync(dto.Email, dto);
            return Ok(new ResponseDto
            {
                isSuccess = true,
                Message = "Password reset successfully",
                Data = response
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new ResponseDto
            {
                isSuccess = false,
                Message = ex.Message,
                Data = null
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
