namespace Service.DTOs.Staff;

/// <summary>
/// DTO để lấy thông tin Staff
/// </summary>
public class GetStaffDto
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
    /// Địa chỉ
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Vai trò
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Mô tả
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
