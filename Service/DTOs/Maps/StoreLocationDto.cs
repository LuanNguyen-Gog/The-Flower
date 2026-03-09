namespace Service.DTOs.Maps;

public class StoreLocationDto
{
    public int LocationId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
}
