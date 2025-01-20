using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class CategoryRepository(ApplicationDbContext context) : BaseRepository<Category>(context), ICategoryRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<Category>> GetCategoryTreeAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .ToListAsync();

        return categories;
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<List<Category>> GetChildrenAsync(int parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .ToListAsync();
    }

    public async Task<bool> HasChildrenAsync(int categoryId)
    {
        return await _context.Categories
            .AnyAsync(c => c.ParentId == categoryId);
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync()
    {
        return await _context.Set<CategoryHierarchyDto>()
            .FromSqlRaw("EXEC GetRecursiveCategories")
            .ToListAsync();
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        if (await HasChildrenAsync(id))
        {
            throw new BusinessRuleException("Cannot delete category with child categories");
        }

        if (await HasProductsAsync(id))
        {
            throw new BusinessRuleException("Cannot delete category with existing products");
        }

        return await base.DeleteAsync(id);
    }
}