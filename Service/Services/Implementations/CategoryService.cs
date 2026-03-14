using Repository.Models;
using Repository.Repositories.Interfaces;
using Service.DTOs.Categories;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
        => _categoryRepository = categoryRepository;

    public async Task<IEnumerable<GetCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<GetCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? null : MapToDto(category);
    }

    public async Task<GetCategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName))
            throw new InvalidOperationException("Category name is required.");

        var category = new Category
        {
            CategoryName = dto.CategoryName,
            Status = "Active"
        };

        var createdCategory = await _categoryRepository.CreateAsync(category);
        return MapToDto(createdCategory);
    }

    public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CategoryName))
            throw new InvalidOperationException("Category name is required.");

        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        category.CategoryName = dto.CategoryName;
        category.Status = dto.Status;

        return await _categoryRepository.UpdateAsync(category);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        return await _categoryRepository.DeleteAsync(id);
    }

    private static GetCategoryDto MapToDto(Category category) => new()
    {
        CategoryId = category.CategoryId,
        CategoryName = category.CategoryName,
        Status = category.Status
    };
}
