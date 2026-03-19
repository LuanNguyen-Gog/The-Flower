using Service.DTOs.Auth;
using Service.DTOs.Staff;

namespace Service.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto);

    // Staff Management Methods
    Task<CreateStaffResponseDto> CreateStaffAsync(CreateStaffDto dto);
    Task<GetStaffDto> GetStaffByIdAsync(int staffId);
    Task<List<GetStaffDto>> GetAllStaffAsync();
    Task<bool> PatchStaffStatusAsync(int staffId, string status);
}
