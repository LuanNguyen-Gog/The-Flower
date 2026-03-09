namespace Service.DTOs.Products;

public class ProductDetailDto : ProductSummaryDto
{
    public string? FullDescription { get; set; }
    public string? TechnicalSpecifications { get; set; }
}
