using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Application.Interfaces.Services;
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

    public async Task<OperationResult<ProductWithCategoryDto>> GetByIdDetailedAsync(int id)
    {
        var productOperationResult = await _productRepository.GetByIdDetailedAsync(id);

        if (productOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to get non-existent product with ID: {ProductId}", id);
            return OperationResult<ProductWithCategoryDto>.Failure(productOperationResult.ErrorCode,
                productOperationResult.ErrorMessage);
        }

        return OperationResult<ProductWithCategoryDto>.Success(productOperationResult.Data);
    }

    public async Task<OperationResult<ProductDto?>> GetByIdAsync(int id)
    {
        var productOperationResult = await _productRepository.GetByIdAsync(id);
        if (productOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to get non-existent product with ID: {ProductId}", id);
            return OperationResult<ProductDto?>.Failure(productOperationResult.ErrorCode,
                productOperationResult.ErrorMessage);
        }

        var categoryOperationResult = await _categoryRepository.GetByIdAsync(productOperationResult.Data.CategoryId);

        var product = await ProductDto.MapToProductDtoAsync(productOperationResult.Data, categoryOperationResult.Data);

        return OperationResult<ProductDto?>.Success(product);
    }

    public async Task<OperationResult<ProductWithCategoryDto>> GetBySlugAsync(string slug)
    {
        var productOperationResult = await _productRepository.GetBySlugAsync(slug);
        if (productOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to get product by non-existent slug: {Slug}", slug);
            return OperationResult<ProductWithCategoryDto>.Failure(productOperationResult.ErrorCode,
                productOperationResult.ErrorMessage);
        }

        return productOperationResult;
    }

    public async Task<OperationResult<List<ProductDto>>> GetAllAsync()
    {
        var productsOperationResult = await _productRepository.GetAllAsync();
        var categoriesOperationResult = await _categoryRepository.GetAllAsync();

        var products = productsOperationResult.Data.Select(async product =>
        {
            var category = categoriesOperationResult.Data.FirstOrDefault(c => c.Id == product.CategoryId);
            return await ProductDto.MapToProductDtoAsync(product, category);
        }).Select(t => t.Result).ToList();

        return OperationResult<List<ProductDto>>.Success(products);
    }

    public async Task<OperationResult<List<ProductDto>>> GetByCategoryIdAsync(int categoryId)
    {
        var isCategoryExistsOperationResult = await _categoryRepository.ExistsAsync(categoryId);
        if (isCategoryExistsOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to get products by non-existent category ID: {CategoryId}", categoryId);
            return OperationResult<List<ProductDto>>.Failure(isCategoryExistsOperationResult.ErrorCode,
                isCategoryExistsOperationResult.ErrorMessage);
        }

        var productsOperationResult = await _productRepository.GetByCategoryIdAsync(categoryId);
        var categoriesOperationResult = await _categoryRepository.GetAllAsync();

        var products = productsOperationResult.Data.Select(async product =>
        {
            var category = categoriesOperationResult.Data.FirstOrDefault(c => c.Id == product.CategoryId);
            return await ProductDto.MapToProductDtoAsync(product, category);
        }).Select(t => t.Result).ToList();

        return OperationResult<List<ProductDto>>.Success(products);
    }

    public async Task<OperationResult<ProductDto>> CreateAsync(CreateProductDto createProductDto)
    {
        var categoryOperationResult = await _categoryRepository.GetByIdAsync(createProductDto.CategoryId);
        if (categoryOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to create product with non-existent category ID: {CategoryId}",
                createProductDto.CategoryId);
            return OperationResult<ProductDto>.Failure(categoryOperationResult.ErrorCode,
                categoryOperationResult.ErrorMessage);
        }

        if (createProductDto.StockQuantity < 0)
        {
            return OperationResult<ProductDto>.Failure(ErrorCode.InvalidStockQuantity,
                "Stock quantity cannot be negative");
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

        var createdProductOperationResult = await _productRepository.AddAsync(product);

        if (createdProductOperationResult.IsSuccess == false)
        {
            return OperationResult<ProductDto>.Failure(createdProductOperationResult.ErrorCode,
                createdProductOperationResult.ErrorMessage);
        }

        _logger.LogInformation("Created new product with ID: {ProductId}", createdProductOperationResult.Data.Id);

        var productDto =
            await ProductDto.MapToProductDtoAsync(createdProductOperationResult.Data, categoryOperationResult.Data);

        return OperationResult<ProductDto>.Success(productDto);
    }

    public async Task<OperationResult<ProductDto>> UpdateAsync(int id, UpdateProductDto updateProductDto)
    {
        var productOperationResult = await _productRepository.GetByIdAsync(id);
        if (productOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to update non-existent product with ID: {ProductId}", id);
            return OperationResult<ProductDto>.Failure(productOperationResult.ErrorCode,
                productOperationResult.ErrorMessage);
        }

        if (productOperationResult.Data.CategoryId != updateProductDto.CategoryId)
        {
            var categoryExistsOperationResult = await _categoryRepository.ExistsAsync(updateProductDto.CategoryId);
            if (categoryExistsOperationResult.IsSuccess == false)
            {
                _logger.LogWarning("Attempted to update product with non-existent category ID: {CategoryId}",
                    updateProductDto.CategoryId);
                return OperationResult<ProductDto>.Failure(categoryExistsOperationResult.ErrorCode,
                    categoryExistsOperationResult.ErrorMessage);
            }
        }

        if (updateProductDto.StockQuantity < 0)
        {
            return OperationResult<ProductDto>.Failure(ErrorCode.InvalidStockQuantity,
                "Stock quantity cannot be negative");
        }

        productOperationResult.Data.Name = updateProductDto.Name;
        productOperationResult.Data.Description = updateProductDto.Description ?? string.Empty;
        productOperationResult.Data.Price = updateProductDto.Price;
        productOperationResult.Data.ImageUrl = updateProductDto.ImageUrl ?? string.Empty;
        productOperationResult.Data.CategoryId = updateProductDto.CategoryId;
        productOperationResult.Data.Slug = GenerateSlug(updateProductDto.Name);

        await UpdateStockAndStatus(productOperationResult.Data, updateProductDto.StockQuantity,
            updateProductDto.BusinessStatus);

        var updatedProductOperationResult = await _productRepository.UpdateAsync(productOperationResult.Data);

        if (updatedProductOperationResult.IsSuccess == false)
        {
            return OperationResult<ProductDto>.Failure(updatedProductOperationResult.ErrorCode,
                updatedProductOperationResult.ErrorMessage);
        }

        _logger.LogInformation("Updated product with ID: {ProductId}", id);

        var categoryOperationResult =
            await _categoryRepository.GetByIdAsync(updatedProductOperationResult.Data.CategoryId);

        if (categoryOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Product updated with non-existent category ID: {CategoryId}",
                updatedProductOperationResult.Data.CategoryId);
            return OperationResult<ProductDto>.Failure(categoryOperationResult.ErrorCode,
                categoryOperationResult.ErrorMessage);
        }

        var product =
            await ProductDto.MapToProductDtoAsync(updatedProductOperationResult.Data, categoryOperationResult.Data);

        return OperationResult<ProductDto>.Success(product);
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        var deleteOperationResult = await _productRepository.DeleteAsync(id);
        if (deleteOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to delete non-existent product with ID: {ProductId}", id);
            return OperationResult<bool>.Failure(deleteOperationResult.ErrorCode, deleteOperationResult.ErrorMessage);
        }

        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<List<ProductDto>>> SearchAsync(string searchTerm, int? categoryId = null)
    {
        if (categoryId.HasValue)
        {
            var categoryExistsOperationResult = await _categoryRepository.ExistsAsync(categoryId.Value);
            if (categoryExistsOperationResult.IsSuccess == false)
            {
                _logger.LogWarning("Attempted to search products by non-existent category ID: {CategoryId}",
                    categoryId);
                return OperationResult<List<ProductDto>>.Failure(categoryExistsOperationResult.ErrorCode,
                    categoryExistsOperationResult.ErrorMessage);
            }
        }

        var productsOperationResult = await _productRepository.SearchProductsAsync(searchTerm, categoryId);

        if (productsOperationResult.IsSuccess == false)
        {
            return OperationResult<List<ProductDto>>.Failure(productsOperationResult.ErrorCode,
                productsOperationResult.ErrorMessage);
        }

        var products = productsOperationResult.Data.Select(async product =>
        {
            var categoryOperationResult = await _categoryRepository.GetByIdAsync(product.CategoryId);

            return await ProductDto.MapToProductDtoAsync(product, categoryOperationResult.Data);
        }).Select(t => t.Result).ToList();

        return OperationResult<List<ProductDto>>.Success(products);
    }

    public async Task<PaginatedOperationResult<(List<ProductWithCategoryDto> data, int total)>>
        GetProductsWithCategoriesAsync(
            int pageNumber = 1, int pageSize = 10, int? categoryId = null, string? searchText = null)
    {
        var productsDataAndTotalOperationResult =
            await _productRepository.GetProductsWithCategoriesAsync(pageNumber, pageSize, categoryId, searchText);

        return productsDataAndTotalOperationResult;
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