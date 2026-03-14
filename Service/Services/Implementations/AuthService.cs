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
using Service.EmailTemplates;

namespace Service.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<OtpResponse> RegisterAsync(RegisterDto dto)
    {
        // Validate email and username don't already exist
        if (await _userRepository.GetByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email already in use.");

        if (await _userRepository.GetByUsernameAsync(dto.Username) != null)
            throw new InvalidOperationException("Username already taken.");

        // Create pending user (not verified yet)
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Role = "Customer",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Generate 6-digit OTP
        var otpCode = GenerateOtp();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        // Create OTP record
        var otp = new OtpVerification
        {
            Email = dto.Email,
            UserId = user.UserId,
            OtpCode = otpCode,
            ExpiresAt = expiresAt,
            IsUsed = false
        };

        await _otpRepository.AddOtpAsync(otp);
        await _otpRepository.SaveAsync();

        // Send OTP via email
        await _emailService.SendOtpEmailAsync(dto.Email, otpCode);

        return new OtpResponse
        {
            Success = true,
            Message = "OTP sent successfully to your email.",
            ExpiresInMinutes = 10
        };
    }

    public async Task<AuthResponseDto> VerifyOtpAndRegisterAsync(string email, string otpCode, RegisterDto dto)
    {
        // Verify OTP validity
        if (!await _otpRepository.IsOtpValidAsync(email, otpCode))
            throw new InvalidOperationException("Invalid or expired OTP.");

        // Get the pending user
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.Status == "Active")
            throw new InvalidOperationException("Email already verified.");

        // Mark user as active
        user.Status = "Active";
        await _userRepository.UpdateAsync(user);

        // Mark OTP as used
        var otpRecord = await _otpRepository.GetLatestOtpByEmailAsync(email);
        if (otpRecord != null)
        {
            otpRecord.IsUsed = true;
            otpRecord.UsedAt = DateTime.UtcNow;
            await _otpRepository.SaveAsync();
        }

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

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.Username!),
            new Claim(ClaimTypes.Role, user.Role ?? "Customer"),
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

    public async Task<OtpResponse> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Generate 6-digit OTP
        var otpCode = GenerateOtp();
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        // Create OTP record for password reset
        var otp = new OtpVerification
        {
            Email = email,
            UserId = user.UserId,
            OtpCode = otpCode,
            ExpiresAt = expiresAt,
            IsUsed = false
        };

        await _otpRepository.AddOtpAsync(otp);
        await _otpRepository.SaveAsync();

        // Send OTP via email
        await _emailService.SendOtpEmailAsync(email, otpCode);

        return new OtpResponse
        {
            Success = true,
            Message = "OTP sent to your email. Use it to reset your password.",
            ExpiresInMinutes = 10
        };
    }

    public async Task<OtpResponse> VerifyForgotPasswordOtpAsync(string email, string otpCode)
    {
        // Verify OTP validity
        if (!await _otpRepository.IsOtpValidAsync(email, otpCode))
            throw new InvalidOperationException("Invalid or expired OTP.");

        // Mark OTP as used
        var otpRecord = await _otpRepository.GetLatestOtpByEmailAsync(email);
        if (otpRecord != null)
        {
            otpRecord.IsUsed = true;
            otpRecord.UsedAt = DateTime.UtcNow;
            await _otpRepository.SaveAsync();
        }

        return new OtpResponse
        {
            Success = true,
            Message = "OTP verified successfully. You can now reset your password.",
            ExpiresInMinutes = 5
        };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(string email, ResetPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Check if OTP was recently verified (within 5 minutes)
        var recentOtp = await _otpRepository.GetLatestOtpByEmailAsync(email);
        if (recentOtp == null || !recentOtp.IsUsed || recentOtp.UsedAt == null)
            throw new InvalidOperationException("Please verify OTP first.");

        var timeSinceVerified = DateTime.UtcNow - recentOtp.UsedAt.Value;
        if (timeSinceVerified.TotalMinutes > 5)
            throw new InvalidOperationException("OTP verification expired. Please request a new OTP.");

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Verify old password
        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Old password is incorrect.");

        // Update to new password
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

        // Gửi email với thông tin đăng nhập
        await SendStaffCredentialsEmailAsync(dto.Email, dto.Username, temporaryPassword);

        return new CreateStaffResponseDto
        {
            UserId = staff.UserId,
            Username = staff.Username,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            PasswordSent = true,
            Message = "Staff được tạo thành công. Thông tin đăng nhập đã được gửi về email."
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
            throw new InvalidOperationException($"Trạng thái '{status}' không hợp lệ. Các trạng thái hợp lệ: Active, Inactive, Deleted, Pending");

        staff.Status = status;
        await _userRepository.UpdateAsync(staff);

        return true;
    }

    /// <summary>
    /// Kiểm tra trạng thái có hợp lệ không
    /// </summary>
    private bool IsValidStatus(string status)
    {
        var validStatuses = new[] { "Active", "Inactive", "Deleted", "Pending" };
        return validStatuses.Contains(status);
    }

    /// <summary>
    /// Tạo mật khẩu tạm thời
    /// </summary>
    private string GenerateTemporaryPassword()
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
    /// Gửi email với thông tin đăng nhập
    /// </summary>
    private async Task SendStaffCredentialsEmailAsync(string email, string username, string password)
    {
        var template = new StaffCredentialsTemplate(username, password);
        await _emailService.SendEmailAsync(
            email,
            template.GetSubject(),
            template.GetHtmlBody(),
            isHtml: true);
    }

    /// <summary>
    /// Map User to GetStaffDto
    /// </summary>
    private GetStaffDto MapToGetStaffDto(User staff)
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

