namespace Service.DTOs.Maps;

public class UpdateStoreLocationDto
{
    public int LocationId { get; set; }
    public string Address { get; set; } = string.Empty;
}
