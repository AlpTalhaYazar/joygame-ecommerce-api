using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<OperationResult<List<Category>>> GetCategoryTreeAsync(string? slug = null);
    Task<OperationResult<Category?>> GetBySlugAsync(string slug);
    Task<OperationResult<List<Category>>> GetChildrenAsync(int parentId);
    Task<OperationResult<bool>> HasChildrenAsync(int categoryId);
    Task<OperationResult<bool>> HasProductsAsync(int categoryId);
    Task<OperationResult<List<CategoryHierarchyDto>>> GetCategoryHierarchyAsync();
}