using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class CategoryRepository(ApplicationDbContext context) : BaseRepository<Category>(context), ICategoryRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<OperationResult<List<Category>>> GetCategoryTreeAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .ToListAsync();

        if (categories.Count == 0)
            return OperationResult<List<Category>>.Failure(ErrorCode.CategoryNotFound, "No categories found");

        return OperationResult<List<Category>>.Success(categories);
    }

    public async Task<OperationResult<Category?>> GetBySlugAsync(string slug)
    {
        var category = await _context.Categories
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
            return OperationResult<Category?>.Failure(ErrorCode.CategoryNotFound, "Category not found");

        return OperationResult<Category?>.Success(category);
    }

    public async Task<OperationResult<List<Category>>> GetChildrenAsync(int parentId)
    {
        var categories = await _context.Categories
            .Where(c => c.ParentId == parentId)
            .ToListAsync();

        if (categories.Count == 0)
            return OperationResult<List<Category>>.Failure(ErrorCode.CategoryNotFound, "No child categories found");

        return OperationResult<List<Category>>.Success(categories);
    }

    public async Task<OperationResult<bool>> HasChildrenAsync(int categoryId)
    {
        var hasChildren = await _context.Categories
            .AnyAsync(c => c.ParentId == categoryId);

        return OperationResult<bool>.Success(hasChildren);
    }

    public async Task<OperationResult<bool>> HasProductsAsync(int categoryId)
    {
        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);

        return OperationResult<bool>.Success(hasProducts);
    }

    public async Task<OperationResult<List<CategoryHierarchyDto>>> GetCategoryHierarchyAsync()
    {
        var categoriesWithHierarchies = await _context.Set<CategoryHierarchyDto>()
            .FromSqlRaw("EXEC GetRecursiveCategories")
            .ToListAsync();

        if (categoriesWithHierarchies.Count == 0)
            return OperationResult<List<CategoryHierarchyDto>>.Failure(ErrorCode.CategoryNotFound,
                "No categories found");

        return OperationResult<List<CategoryHierarchyDto>>.Success(categoriesWithHierarchies);
    }

    public override async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        if ((await HasChildrenAsync(id)).Data)
        {
            return OperationResult<bool>.Failure(ErrorCode.BusinessRuleViolation,
                "Cannot delete category with child categories");
        }

        if ((await HasProductsAsync(id)).Data)
        {
            return OperationResult<bool>.Failure(ErrorCode.BusinessRuleViolation,
                "Cannot delete category with existing products");
        }

        return await base.DeleteAsync(id);
    }
}