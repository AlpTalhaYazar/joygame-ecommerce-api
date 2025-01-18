using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<List<Category>> GetCategoryTreeAsync();
    Task<Category?> GetBySlugAsync(string slug);
    Task<List<Category>> GetChildrenAsync(int parentId);
    Task<bool> HasChildrenAsync(int categoryId);
    Task<bool> HasProductsAsync(int categoryId);
}