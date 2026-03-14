namespace Service.DTOs.Staff;

/// <summary>
/// DTO cho phản hồi khi tạo Staff
/// </summary>
public class CreateStaffResponseDto
{
    /// <summary>
    /// ID nhân viên
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Tên tài khoản
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Mựcmật khẩu tạm thời đã được gửi về email
    /// </summary>
    public bool PasswordSent { get; set; }

    /// <summary>
    /// Thông báo
    /// </summary>
    public string Message { get; set; }
}
