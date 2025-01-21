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

public class CategoryService(
    ICategoryRepository categoryRepository,
    ILogger<CategoryService> logger)
    : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly ILogger<CategoryService> _logger = logger;

    public async Task<OperationResult<CategoryDto?>> GetByIdAsync(int id)
    {
        var categoryOperationResult = await _categoryRepository.GetByIdAsync(id);

        if (!categoryOperationResult.IsSuccess)
            return OperationResult<CategoryDto?>.Failure(categoryOperationResult.ErrorCode,
                categoryOperationResult.ErrorMessage);

        return OperationResult<CategoryDto?>.Success(CategoryDto.MapToCategoryDto(categoryOperationResult.Data));
    }

    public async Task<OperationResult<CategoryDto>> GetBySlugAsync(string slug)
    {
        var categoryOperationResult = await _categoryRepository.GetBySlugAsync(slug);

        if (!categoryOperationResult.IsSuccess)
            return OperationResult<CategoryDto>.Failure(categoryOperationResult.ErrorCode,
                categoryOperationResult.ErrorMessage);

        return OperationResult<CategoryDto>.Success(CategoryDto.MapToCategoryDto(categoryOperationResult.Data));
    }

    public async Task<OperationResult<List<CategoryDto>>> GetAllAsync()
    {
        var categoriesOperationResult = await _categoryRepository.GetAllAsync();

        if (!categoriesOperationResult.IsSuccess)
            return OperationResult<List<CategoryDto>>.Failure(categoriesOperationResult.ErrorCode,
                categoriesOperationResult.ErrorMessage);

        var categories = categoriesOperationResult.Data.Select(CategoryDto.MapToCategoryDto).ToList();

        return OperationResult<List<CategoryDto>>.Success(categories);
    }

    public async Task<OperationResult<List<CategoryTreeDto>>> GetCategoryTreeAsync(string? slug = null)
    {
        var categoriesOperationResult = await _categoryRepository.GetCategoryTreeAsync(slug);

        if (!categoriesOperationResult.IsSuccess)
            return OperationResult<List<CategoryTreeDto>>.Failure(categoriesOperationResult.ErrorCode,
                categoriesOperationResult.ErrorMessage);

        var categories = BuildCategoryTree(categoriesOperationResult.Data);

        return OperationResult<List<CategoryTreeDto>>.Success(categories);
    }

    public async Task<OperationResult<CategoryDto>> CreateAsync(CreateCategoryDto createCategoryDto)
    {
        if (createCategoryDto.ParentId.HasValue)
        {
            var parentExistsOperationResult = await _categoryRepository.ExistsAsync(createCategoryDto.ParentId.Value);
            if (parentExistsOperationResult.IsSuccess == false)
            {
                _logger.LogWarning("Attempted to create category with non-existent parent ID: {ParentId}",
                    createCategoryDto.ParentId.Value);
                return OperationResult<CategoryDto>.Failure(ErrorCode.EntityNotFound,
                    $"Parent category with ID {createCategoryDto.ParentId} not found");
            }
        }

        var slug = GenerateSlug(createCategoryDto.Name);

        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description ?? String.Empty,
            ParentId = createCategoryDto.ParentId,
            Slug = slug,
            Status = EntityStatus.Active,
            CreatedBy = "System",
        };

        var createdCategoryOperationResult = await _categoryRepository.AddAsync(category);

        if (!createdCategoryOperationResult.IsSuccess)
        {
            return OperationResult<CategoryDto>.Failure(createdCategoryOperationResult.ErrorCode,
                createdCategoryOperationResult.ErrorMessage);
        }

        _logger.LogInformation("Created new category with ID: {CategoryId}", createdCategoryOperationResult.Data.Id);

        var categoryDto = CategoryDto.MapToCategoryDto(createdCategoryOperationResult.Data);

        return OperationResult<CategoryDto>.Success(categoryDto);
    }

    public async Task<OperationResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
        var categoryOperationResult = await _categoryRepository.GetByIdAsync(id);
        if (categoryOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Attempted to update non-existent category with ID: {CategoryId}", id);
            return OperationResult<CategoryDto>.Failure(categoryOperationResult.ErrorCode,
                categoryOperationResult.ErrorMessage);
        }

        if (updateCategoryDto.ParentId.HasValue)
        {
            var parentExistsOperationResult = await _categoryRepository.ExistsAsync(updateCategoryDto.ParentId.Value);
            if (parentExistsOperationResult.IsSuccess == false)
            {
                _logger.LogWarning("Attempted to update category with non-existent parent ID: {ParentId}",
                    updateCategoryDto.ParentId.Value);
                return OperationResult<CategoryDto>.Failure(ErrorCode.EntityNotFound,
                    $"Parent category with ID {updateCategoryDto.ParentId} not found");
            }

            if (await WouldCreateCircularReference(id, updateCategoryDto.ParentId.Value))
            {
                return OperationResult<CategoryDto>.Failure(ErrorCode.BusinessRuleViolation,
                    "Cannot create circular reference");
            }
        }

        categoryOperationResult.Data.Name = updateCategoryDto.Name;
        categoryOperationResult.Data.Description = updateCategoryDto.Description ?? String.Empty;
        categoryOperationResult.Data.ParentId = updateCategoryDto.ParentId;
        categoryOperationResult.Data.Slug = GenerateSlug(updateCategoryDto.Name);

        var updatedCategoryOperationResult = await _categoryRepository.UpdateAsync(categoryOperationResult.Data);

        if (!updatedCategoryOperationResult.IsSuccess)
        {
            return OperationResult<CategoryDto>.Failure(updatedCategoryOperationResult.ErrorCode,
                updatedCategoryOperationResult.ErrorMessage);
        }

        _logger.LogInformation("Updated category with ID: {CategoryId}", id);

        var categoryDto = CategoryDto.MapToCategoryDto(updatedCategoryOperationResult.Data);

        return OperationResult<CategoryDto>.Success(categoryDto);
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        var hasChildrenOperationResult = await _categoryRepository.HasChildrenAsync(id);
        if (hasChildrenOperationResult.Data)
        {
            _logger.LogWarning("Attempted to delete category with children. Category ID: {CategoryId}", id);
            return OperationResult<bool>.Failure(ErrorCode.BusinessRuleViolation,
                "Cannot delete category with child categories");
        }

        var hasProductsOperationResult = await _categoryRepository.HasProductsAsync(id);
        if (hasProductsOperationResult.Data)
        {
            _logger.LogWarning("Attempted to delete category with products. Category ID: {CategoryId}", id);
            return OperationResult<bool>.Failure(ErrorCode.BusinessRuleViolation,
                "Cannot delete category with products");
        }

        var deleteOperationResult = await _categoryRepository.DeleteAsync(id);
        if (deleteOperationResult.IsSuccess == false)
        {
            _logger.LogWarning("Failed to delete category with ID: {CategoryId}", id);
            return OperationResult<bool>.Failure(deleteOperationResult.ErrorCode, deleteOperationResult.ErrorMessage);
        }

        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<List<CategoryHierarchyDto>>> GetCategoryHierarchyAsync()
    {
        var categoriesOperationResult = await _categoryRepository.GetCategoryHierarchyAsync();

        if (!categoriesOperationResult.IsSuccess)
            return OperationResult<List<CategoryHierarchyDto>>.Failure(categoriesOperationResult.ErrorCode,
                categoriesOperationResult.ErrorMessage);

        return OperationResult<List<CategoryHierarchyDto>>.Success(categoriesOperationResult.Data);
    }

    private static List<CategoryTreeDto> BuildCategoryTree(List<Category> categories, int? parentId = null)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                ParentId = c.ParentId,
                Children = BuildCategoryTree(categories, c.Id)
            })
            .ToList();
    }

    private async Task<bool> WouldCreateCircularReference(int categoryId, int newParentId)
    {
        var currentParentId = newParentId;
        var visitedIds = new HashSet<int> { categoryId };

        while (currentParentId > 0)
        {
            if (!visitedIds.Add(currentParentId))
            {
                return true;
            }

            var parentOperationResult = await _categoryRepository.GetByIdAsync(currentParentId);
            if (parentOperationResult.IsSuccess == false || parentOperationResult.Data == null)
            {
                break;
            }

            currentParentId = parentOperationResult.Data.ParentId.Value;
        }

        return false;
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