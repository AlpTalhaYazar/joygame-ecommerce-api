using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<List<CategoryDto>> GetAllAsync();
    Task<List<CategoryTreeDto>> GetCategoryTreeAsync();
    Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto);
    Task<bool> DeleteAsync(int id);
}