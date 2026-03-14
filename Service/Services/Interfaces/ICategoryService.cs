using Repository.Models;
using Service.DTOs.Categories;

namespace Service.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<GetCategoryDto>> GetAllCategoriesAsync();
    Task<GetCategoryDto?> GetCategoryByIdAsync(int id);
    Task<GetCategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}
