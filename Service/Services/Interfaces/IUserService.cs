using Service.DTOs.Users;

namespace Service.Services.Interfaces;

public interface IUserService
{
    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto> CreateUserAsync(CreateAdminUserDto dto);
    Task<AdminUserDto> UpdateUserAsync(Guid userId, UpdateAdminUserDto dto);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<UserProfileDto> GetUserProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(Guid userId, ChangeUserPasswordDto dto);
}
