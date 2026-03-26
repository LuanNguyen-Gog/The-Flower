namespace Service.DTOs.Maps;

public class UpdateStoreLocationDto
{
    public Guid LocationId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
