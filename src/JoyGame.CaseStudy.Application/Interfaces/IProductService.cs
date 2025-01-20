using JoyGame.CaseStudy.Application.DTOs;

namespace JoyGame.CaseStudy.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    Task<List<ProductDto>> GetAllAsync();
    Task<List<ProductDto>> GetByCategoryIdAsync(int categoryId);
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto updateProductDto);
    Task<bool> DeleteAsync(int id);
    Task<List<ProductDto>> SearchAsync(string searchTerm, int? categoryId = null);
    Task<List<ProductWithCategoryDto>> GetProductsWithCategoriesAsync();
}