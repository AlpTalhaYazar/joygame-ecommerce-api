using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
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

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? CategoryDto.MapToCategoryDto(category) : null;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(CategoryDto.MapToCategoryDto).ToList();
    }

    public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        var categories = await _categoryRepository.GetCategoryTreeAsync();

        return BuildCategoryTree(categories);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto)
    {
        if (createCategoryDto.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(createCategoryDto.ParentId.Value);
            if (!parentExists)
            {
                _logger.LogWarning("Attempted to create category with non-existent parent ID: {ParentId}",
                    createCategoryDto.ParentId.Value);
                throw new BusinessRuleException("Parent category does not exist");
            }
        }

        var slug = GenerateSlug(createCategoryDto.Name);

        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description ?? String.Empty,
            ParentId = createCategoryDto.ParentId ?? 0,
            Slug = slug,
            Status = EntityStatus.Active,
            CreatedBy = "System",
        };

        var createdCategory = await _categoryRepository.AddAsync(category);
        _logger.LogInformation("Created new category with ID: {CategoryId}", createdCategory.Id);

        return CategoryDto.MapToCategoryDto(createdCategory);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Attempted to update non-existent category with ID: {CategoryId}", id);
            throw new EntityNotFoundException(nameof(Category), id);
        }

        if (updateCategoryDto.ParentId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(updateCategoryDto.ParentId.Value);
            if (!parentExists)
            {
                throw new BusinessRuleException("Parent category does not exist");
            }

            if (await WouldCreateCircularReference(id, updateCategoryDto.ParentId.Value))
            {
                throw new BusinessRuleException("Cannot set parent category as it would create a circular reference");
            }
        }

        category.Name = updateCategoryDto.Name;
        category.Description = updateCategoryDto.Description ?? String.Empty;
        category.ParentId = updateCategoryDto.ParentId ?? 0;
        category.Slug = GenerateSlug(updateCategoryDto.Name);

        var updatedCategory = await _categoryRepository.UpdateAsync(category);
        _logger.LogInformation("Updated category with ID: {CategoryId}", id);

        return CategoryDto.MapToCategoryDto(updatedCategory);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hasChildren = await _categoryRepository.HasChildrenAsync(id);
        if (hasChildren)
        {
            _logger.LogWarning("Attempted to delete category with children. Category ID: {CategoryId}", id);
            throw new BusinessRuleException("Cannot delete category that has child categories");
        }

        var hasProducts = await _categoryRepository.HasProductsAsync(id);
        if (hasProducts)
        {
            _logger.LogWarning("Attempted to delete category with products. Category ID: {CategoryId}", id);
            throw new BusinessRuleException("Cannot delete category that has products");
        }

        var result = await _categoryRepository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation("Deleted category with ID: {CategoryId}", id);
        }

        return result;
    }

    private static List<CategoryTreeDto> BuildCategoryTree(List<Category> categories, int? parentId = null)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
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

        while (currentParentId != 0)
        {
            if (!visitedIds.Add(currentParentId))
            {
                return true;
            }

            var parent = await _categoryRepository.GetByIdAsync(currentParentId);
            if (parent == null)
            {
                break;
            }

            currentParentId = parent.ParentId;
        }

        return false;
    }

    private static string GenerateSlug(string name)
    {
        // Convert spaces to dashes and remove special characters
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