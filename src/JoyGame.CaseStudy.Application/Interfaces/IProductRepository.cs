using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
    Task<List<Product>> GetByCategoryIdAsync(int categoryId);
    Task<List<Product>> GetActiveByCategoryIdAsync(int categoryId);
    Task<List<Product>> SearchProductsAsync(string searchTerm, int? categoryId = null);
}