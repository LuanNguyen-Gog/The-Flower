namespace Service.DTOs.Categories;

public class GetCategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
