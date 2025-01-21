using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces.Services;

public interface IProductService
{
    Task<OperationResult<ProductWithCategoryDto>> GetByIdDetailedAsync(int id);
    Task<OperationResult<ProductDto?>> GetByIdAsync(int id);
    Task<OperationResult<ProductDto?>> GetBySlugAsync(string slug);
    Task<OperationResult<List<ProductDto>>> GetAllAsync();
    Task<OperationResult<List<ProductDto>>> GetByCategoryIdAsync(int categoryId);
    Task<OperationResult<ProductDto>> CreateAsync(CreateProductDto createProductDto);
    Task<OperationResult<ProductDto>> UpdateAsync(int id, UpdateProductDto updateProductDto);
    Task<OperationResult<bool>> DeleteAsync(int id);
    Task<OperationResult<List<ProductDto>>> SearchAsync(string searchTerm, int? categoryId = null);

    Task<PaginatedOperationResult<(List<ProductWithCategoryDto> data, int total)>> GetProductsWithCategoriesAsync(
        int pageNumber = 1, int pageSize = 10, int? categoryId = null, string? searchText = null);
}