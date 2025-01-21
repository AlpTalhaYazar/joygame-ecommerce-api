using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<OperationResult<CategoryDto?>> GetByIdAsync(int id);
    Task<OperationResult<CategoryDto>> GetBySlugAsync(string slug);
    Task<OperationResult<List<CategoryDto>>> GetAllAsync();
    Task<OperationResult<List<CategoryTreeDto>>> GetCategoryTreeAsync();
    Task<OperationResult<CategoryDto>> CreateAsync(CreateCategoryDto createCategoryDto);
    Task<OperationResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto);
    Task<OperationResult<bool>> DeleteAsync(int id);
    Task<OperationResult<List<CategoryHierarchyDto>>> GetCategoryHierarchyAsync();
}