namespace Service.DTOs.Staff;

/// <summary>
/// DTO để tạo Staff mới
/// </summary>
public class CreateStaffDto
{
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
    /// Mô tả
    /// </summary>
    public string Description { get; set; }
}
