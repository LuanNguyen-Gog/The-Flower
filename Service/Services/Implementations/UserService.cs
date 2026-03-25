using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Users;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users
            .Where(u => !string.Equals(u.Status, "InActive", StringComparison.OrdinalIgnoreCase))
            .Select(MapToAdminDto)
            .ToList();
    }

    public async Task<AdminUserDto> CreateUserAsync(CreateAdminUserDto dto)
    {
        if (await _userRepository.GetByEmailAsync(dto.Email) is not null)
            throw new InvalidOperationException("Email already in use.");

        if (await _userRepository.GetByUsernameAsync(dto.Username) is not null)
            throw new InvalidOperationException("Username already taken.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role,
            Status = "Active",
            CreatedAt = DateTime.UtcNow.AddHours(7)
        };

        var created = await _userRepository.CreateAsync(user);
        return MapToAdminDto(created);
    }

    public async Task<AdminUserDto> UpdateUserAsync(Guid userId, UpdateAdminUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var existingByEmail = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingByEmail is not null && existingByEmail.UserId != userId)
            throw new InvalidOperationException("Email already in use.");

        var existingByUsername = await _userRepository.GetByUsernameAsync(dto.Username);
        if (existingByUsername is not null && existingByUsername.UserId != userId)
            throw new InvalidOperationException("Username already taken.");

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.Role = string.IsNullOrWhiteSpace(dto.Role) ? user.Role : dto.Role;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var updated = await _userRepository.UpdateAsync(user);
        return MapToAdminDto(updated);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return false;

        user.Status = "InActive";
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        // The current schema has no FullName column, so username acts as display name.
        user.Username = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.Description = dto.Avatar;

        var updated = await _userRepository.UpdateAsync(user);
        return MapToProfileDto(updated);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangeUserPasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Old password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    private static AdminUserDto MapToAdminDto(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address,
        Role = user.Role
    };

    private static UserProfileDto MapToProfileDto(User user) => new()
    {
        Id = user.UserId,
        Email = user.Email,
        Username = user.Username,
        FullName = user.Username,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address,
        Avatar = user.Description,
        Role = user.Role,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.CreatedAt
    };
}
