using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Domain.Entities;

namespace JoyGame.CaseStudy.Application.Interfaces.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<OperationResult<ProductWithCategoryDto>> GetByIdDetailedAsync(int id);
    Task<OperationResult<Product?>> GetBySlugAsync(string slug);
    Task<OperationResult<List<Product>>> GetByCategoryIdAsync(int categoryId);
    Task<OperationResult<List<Product>>> GetActiveByCategoryIdAsync(int categoryId);
    Task<OperationResult<List<Product>>> SearchProductsAsync(string searchTerm, int? categoryId = null);

    Task<PaginatedOperationResult<(List<ProductWithCategoryDto> data, int total)>> GetProductsWithCategoriesAsync(
        int pageNumber = 1, int pageSize = 10, int? categoryId = null, string? searchText = null);
}