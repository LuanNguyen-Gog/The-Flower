namespace Service.DTOs.Maps;

public class StoreLocationDto
{
    public Guid LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
