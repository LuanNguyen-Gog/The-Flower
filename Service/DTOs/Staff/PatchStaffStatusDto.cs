namespace Service.DTOs.Staff;

/// <summary>
/// DTO để cập nhật trạng thái Staff
/// </summary>
public class PatchStaffStatusDto
{
    /// <summary>
    /// Trạng thái mới (Active, Inactive, Deleted, Pending)
    /// </summary>
    public string Status { get; set; }
}
