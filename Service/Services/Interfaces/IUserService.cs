using Service.DTOs.Users;

namespace Service.Services.Interfaces;

public interface IUserService
{
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto> CreateUserAsync(CreateAdminUserDto dto);
    Task<AdminUserDto> UpdateUserAsync(int userId, UpdateAdminUserDto dto);
    Task<bool> DeleteUserAsync(int userId);
    Task<UserProfileDto> GetUserProfileAsync(int userId);
    Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(int userId, ChangeUserPasswordDto dto);
}
