namespace Service.DTOs.Categories;

public class UpdateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
