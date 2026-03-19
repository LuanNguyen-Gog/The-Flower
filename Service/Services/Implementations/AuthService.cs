using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Auth;
using Service.DTOs.Staff;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Validate email and username don't already exist
        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email already in use.");

        if (await _userRepository.GetByUsernameAsync(dto.Username) != null)
            throw new InvalidOperationException("Username already taken.");

        // Create new user and set status as Active
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Role = "Customer",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username!,
        Email = user.Email!,
        Role = user.Role ?? "Customer",
        Token = GenerateJwtToken(user)
    };

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Old password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return BuildResponse(user);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STAFF MANAGEMENT METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Tạo Staff mới
    /// </summary>
    public async Task<CreateStaffResponseDto> CreateStaffAsync(CreateStaffDto dto)
    {
        // Validate email đã tồn tại
        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email đã được sử dụng.");

        // Validate username đã tồn tại
        if (await _userRepository.GetByUsernameAsync(dto.Username) != null)
            throw new InvalidOperationException("Tên tài khoản đã tồn tại.");

        // Tạo mật khẩu tạm thời ngẫu nhiên
        var temporaryPassword = GenerateTemporaryPassword();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

        // Tạo Staff mới
        var staff = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Role = "Staff",
            Status = "Active",
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(staff);

        return new CreateStaffResponseDto
        {
            UserId = staff.UserId,
            Username = staff.Username,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber ?? string.Empty,
            PasswordSent = false,
            TemporaryPassword = temporaryPassword,
            Message = "Staff được tạo thành công. Vui lòng bàn giao mật khẩu tạm thời cho staff theo kênh nội bộ."
        };
    }

    /// <summary>
    /// Lấy thông tin Staff theo ID
    /// </summary>
    public async Task<GetStaffDto> GetStaffByIdAsync(int staffId)
    {
        var staff = await _userRepository.GetByIdAsync(staffId);
        if (staff == null || staff.Role != "Staff")
            throw new InvalidOperationException("Không tìm thấy Staff.");

        return MapToGetStaffDto(staff);
    }

    /// <summary>
    /// Lấy danh sách tất cả Staff
    /// </summary>
    public async Task<List<GetStaffDto>> GetAllStaffAsync()
    {
        var staffList = await _userRepository.GetAllByRoleAsync("Staff");
        return staffList.Select(MapToGetStaffDto).ToList();
    }

    /// <summary>
    /// Cập nhật trạng thái Staff
    /// </summary>
    public async Task<bool> PatchStaffStatusAsync(int staffId, string status)
    {
        var staff = await _userRepository.GetByIdAsync(staffId);
        if (staff == null || staff.Role != "Staff")
            throw new InvalidOperationException("Không tìm thấy Staff.");

        if (!IsValidStatus(status))
            throw new InvalidOperationException($"Trạng thái '{status}' không hợp lệ. Các trạng thái hợp lệ: Active, Inactive, Deleted");

        staff.Status = status;
        await _userRepository.UpdateAsync(staff);

        return true;
    }

    /// <summary>
    /// Kiểm tra trạng thái có hợp lệ không
    /// </summary>
    private static bool IsValidStatus(string status)
    {
        var validStatuses = new[] { "Active", "Inactive", "Deleted" };
        return validStatuses.Contains(status);
    }

    /// <summary>
    /// Tạo mật khẩu tạm thời
    /// </summary>
    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var random = new Random();
        var password = new char[12];

        for (int i = 0; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }

        return new string(password);
    }

    /// <summary>
    /// Map User to GetStaffDto
    /// </summary>
    private static GetStaffDto MapToGetStaffDto(User staff)
    {
        return new GetStaffDto
        {
            UserId = staff.UserId,
            Username = staff.Username,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            Address = staff.Address,
            Role = staff.Role,
            Status = staff.Status,
            Description = staff.Description,
            CreatedAt = staff.CreatedAt
        };
    }
}

