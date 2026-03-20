namespace Service.DTOs.Products;

public class ProductSummaryDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BriefDescription { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
