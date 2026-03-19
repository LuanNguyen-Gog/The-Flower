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
    /// Đã gửi mật khẩu qua email hay chưa
    /// </summary>
    public bool PasswordSent { get; set; }

    /// <summary>
    /// Mật khẩu tạm thời để bàn giao thủ công
    /// </summary>
    public string TemporaryPassword { get; set; }

    /// <summary>
    /// Thông báo
    /// </summary>
    public string Message { get; set; }
}
