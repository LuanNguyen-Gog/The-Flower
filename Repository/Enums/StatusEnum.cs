namespace Repository.Enums;

/// <summary>
/// Enum định nghĩa trạng thái cho soft delete
/// </summary>
public enum StatusEnum
{
    /// <summary>
    /// Đang chờ xác minh
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Đã hoạt động
    /// </summary>
    Active = 1,

    /// <summary>
    /// Đã xóa mềm
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// Bị chặn
    /// </summary>
    Inactive = 3
}
