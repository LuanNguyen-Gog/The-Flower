using Repository.Models;
using Service.DTOs.Categories;

namespace Service.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<GetCategoryDto>> GetAllCategoriesAsync();
    Task<GetCategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<GetCategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(Guid id);
}
