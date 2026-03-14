using Service.DTOs.Auth;

namespace Service.Services.Interfaces;

public interface IAuthService
{
    Task<OtpResponse> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> VerifyOtpAndRegisterAsync(string email, string otpCode, RegisterDto dto);
    Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<OtpResponse> ForgotPasswordAsync(string email);
    Task<OtpResponse> VerifyForgotPasswordOtpAsync(string email, string otpCode);
    Task<AuthResponseDto> ResetPasswordAsync(string email, ResetPasswordDto dto);
}
