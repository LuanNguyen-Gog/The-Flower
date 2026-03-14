using Service.DTOs.Auth;
using Service.DTOs.Staff;

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

    // Staff Management Methods
    Task<CreateStaffResponseDto> CreateStaffAsync(CreateStaffDto dto);
    Task<GetStaffDto> GetStaffByIdAsync(int staffId);
    Task<List<GetStaffDto>> GetAllStaffAsync();
    Task<bool> PatchStaffStatusAsync(int staffId, string status);
}
