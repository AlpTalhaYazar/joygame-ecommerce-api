using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
    Task<List<Product>> GetByCategoryIdAsync(int categoryId);
    Task<List<Product>> GetActiveByCategoryIdAsync(int categoryId);
    Task<List<Product>> SearchProductsAsync(string searchTerm, int? categoryId = null);
    Task<(List<ProductWithCategoryDto> data, int total)> GetProductsWithCategoriesAsync(int pageNumber = 1, int pageSize = 10, int? categoryId = null);
}