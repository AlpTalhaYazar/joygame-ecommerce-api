using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JoyGame.CaseStudy.Infrastructure.Services;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ILogger<ProductService> logger)
    : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ILogger<ProductService> _logger = logger;

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        return await ProductDto.MapToProductDtoAsync(product, category);
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug)
    {
        var product = await _productRepository.GetBySlugAsync(slug);
        if (product == null)
        {
            return null;
        }

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        return await ProductDto.MapToProductDtoAsync(product, category);
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var categories = await _categoryRepository.GetAllAsync();

        return products.Select(async product =>
        {
            var category = categories.FirstOrDefault(c => c.Id == product.CategoryId);
            return await ProductDto.MapToProductDtoAsync(product, category);
        }).Select(t => t.Result).ToList();
    }

    public async Task<List<ProductDto>> GetByCategoryIdAsync(int categoryId)
    {
        var isCategoryExists = await _categoryRepository.ExistsAsync(categoryId);
        if (!isCategoryExists)
        {
            _logger.LogWarning("Attempted to get products by non-existent category ID: {CategoryId}", categoryId);
            throw new BusinessRuleException("Category does not exist");
        }

        var products = _productRepository.GetByCategoryIdAsync(categoryId);
        var categories = _categoryRepository.GetAllAsync();

        return products.Result.Select(async product =>
        {
            var category = categories.Result.FirstOrDefault(c => c.Id == product.CategoryId);
            return await ProductDto.MapToProductDtoAsync(product, category);
        }).Select(t => t.Result).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        var category = await _categoryRepository.GetByIdAsync(createProductDto.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Attempted to create product with non-existent category ID: {CategoryId}",
                createProductDto.CategoryId);
            throw new BusinessRuleException("Selected category does not exist");
        }

        if (createProductDto.StockQuantity < 0)
        {
            throw new BusinessRuleException("Stock quantity cannot be negative");
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description ?? string.Empty,
            Price = createProductDto.Price,
            ImageUrl = createProductDto.ImageUrl ?? string.Empty,
            CategoryId = createProductDto.CategoryId,
            StockQuantity = createProductDto.StockQuantity,
            Slug = GenerateSlug(createProductDto.Name),
            Status = EntityStatus.Active,
            BusinessStatus = ProductStatus.Draft,
            CreatedBy = "System",
        };

        if (createProductDto.StockQuantity > 0)
        {
            product.BusinessStatus = ProductStatus.Available;
        }
        else
        {
            product.BusinessStatus = ProductStatus.OutOfStock;
        }

        var createdProduct = await _productRepository.AddAsync(product);
        _logger.LogInformation("Created new product with ID: {ProductId}", createdProduct.Id);

        return await ProductDto.MapToProductDtoAsync(createdProduct, category);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Attempted to update non-existent product with ID: {ProductId}", id);
            throw new EntityNotFoundException(nameof(Product), id);
        }

        if (product.CategoryId != updateProductDto.CategoryId)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(updateProductDto.CategoryId);
            if (!categoryExists)
            {
                throw new BusinessRuleException("Selected category does not exist");
            }
        }

        if (updateProductDto.StockQuantity < 0)
        {
            throw new BusinessRuleException("Stock quantity cannot be negative");
        }

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description ?? string.Empty;
        product.Price = updateProductDto.Price;
        product.ImageUrl = updateProductDto.ImageUrl ?? string.Empty;
        product.CategoryId = updateProductDto.CategoryId;
        product.Slug = GenerateSlug(updateProductDto.Name);

        await UpdateStockAndStatus(product, updateProductDto.StockQuantity, updateProductDto.BusinessStatus);

        var updatedProduct = await _productRepository.UpdateAsync(product);
        _logger.LogInformation("Updated product with ID: {ProductId}", id);

        var category = await _categoryRepository.GetByIdAsync(updatedProduct.CategoryId);

        return await ProductDto.MapToProductDtoAsync(updatedProduct, category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _productRepository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Deleted product with ID: {ProductId}", id);
        }

        return result;
    }

    public async Task<List<ProductDto>> SearchAsync(string searchTerm, int? categoryId = null)
    {
        if (categoryId.HasValue)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(categoryId.Value);
            if (!categoryExists)
            {
                throw new EntityNotFoundException(nameof(Category), categoryId.Value);
            }
        }

        var products = await _productRepository.SearchProductsAsync(searchTerm, categoryId);
        return products.Select(async product =>
        {
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            return await ProductDto.MapToProductDtoAsync(product, category);
        }).Select(t => t.Result).ToList();
    }

    public async Task<List<ProductWithCategoryDto>> GetProductsWithCategoriesAsync()
    {
        var products = await _productRepository.GetProductsWithCategoriesAsync();

        return products;
    }

    private async Task UpdateStockAndStatus(Product product, int newStockQuantity, ProductStatus newStatus)
    {
        var oldStockQuantity = product.StockQuantity;
        product.StockQuantity = newStockQuantity;

        if (oldStockQuantity != newStockQuantity)
        {
            _logger.LogInformation(
                "Stock updated for product {ProductId}: {OldStock} -> {NewStock}",
                product.Id, oldStockQuantity, newStockQuantity);
        }

        product.BusinessStatus = newStatus;

        if (newStatus == ProductStatus.Available && newStockQuantity == 0)
        {
            _logger.LogWarning(
                "Product {ProductId} marked as Available but has no stock", product.Id);
            throw new BusinessRuleException(
                "Cannot set product status to Available when stock quantity is 0");
        }

        if (newStatus != ProductStatus.Discontinued)
        {
            if (newStockQuantity == 0)
            {
                product.BusinessStatus = ProductStatus.OutOfStock;
            }
            else if (product.BusinessStatus == ProductStatus.OutOfStock)
            {
                product.BusinessStatus = ProductStatus.Available;
            }
        }
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(":", "-")
            .Replace(";", "-")
            .Replace("!", "-")
            .Replace("?", "-")
            .Replace(",", "-")
            .Replace("\"", "")
            .Replace("'", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("@", "")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("%", "")
            .Replace("^", "")
            .Replace("&", "")
            .Replace("*", "")
            .Replace("+", "")
            .Replace("=", "")
            .Replace("|", "")
            .Replace("`", "")
            .Replace("~", "")
            .Replace("<", "")
            .Replace(">", "");
    }
}