namespace Service.DTOs.Products;

public class CreateProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? BriefDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? TechnicalSpecifications { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public int? StockQuantity { get; set; }
}
