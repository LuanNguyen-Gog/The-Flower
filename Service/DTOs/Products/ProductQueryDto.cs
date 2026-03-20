namespace Service.DTOs.Products;

public class ProductQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    /// <summary>Hợp lệ: "price" | "name"</summary>
    public string? SortBy { get; set; }

    /// <summary>Hợp lệ: "asc" | "desc"</summary>
    public string? SortOrder { get; set; } = "asc";
}
