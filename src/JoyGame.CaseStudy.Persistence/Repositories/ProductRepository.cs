using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<List<Product>> GetByCategoryIdAsync(int categoryId)
    {
        var categoryIds = await GetCategoryAndChildrenIds(categoryId);

        return await _context.Products
            .Include(p => p.Category)
            .Where(p => categoryIds.Contains(p.CategoryId))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetActiveByCategoryIdAsync(int categoryId)
    {
        var categoryIds = await GetCategoryAndChildrenIds(categoryId);

        return await _context.Products
            .Include(p => p.Category)
            .Where(p => categoryIds.Contains(p.CategoryId))
            .Where(p => p.Status == EntityStatus.Active)
            .Where(p => p.BusinessStatus == ProductStatus.Available)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm, int? categoryId = null)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category);

        if (categoryId.HasValue)
        {
            var categoryIds = await GetCategoryAndChildrenIds(categoryId.Value);
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{searchTerm}%") || (String.IsNullOrWhiteSpace(p.Description) &&
                                                                 EF.Functions.Like(p.Description, $"%{searchTerm}%")));
        }

        query = query.Where(p => p.Status == EntityStatus.Active);

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    private async Task<List<int>> GetCategoryAndChildrenIds(int categoryId)
    {
        var categories = await _context.Categories.ToListAsync();
        var result = new List<int> { categoryId };

        void AddChildrenIds(int parentId)
        {
            var childrenIds = categories
                .Where(c => c.ParentId == parentId)
                .Select(c => c.Id);

            foreach (var childId in childrenIds)
            {
                result.Add(childId);
                AddChildrenIds(childId);
            }
        }

        AddChildrenIds(categoryId);
        return result;
    }

    public async Task<(List<ProductWithCategoryDto> data, int total)> GetProductsWithCategoriesAsync(int pageNumber = 1,
        int pageSize = 10)
    {
        var sql = "EXEC GetProductsWithCategories @PageNumber, @PageSize";
        var pageNumberParam = new SqlParameter("@PageNumber", pageNumber);
        var pageSizeParam = new SqlParameter("@PageSize", pageSize);

        return (await _context.Set<ProductWithCategoryDto>()
                .FromSqlRaw(sql, pageNumberParam, pageSizeParam)
                .ToListAsync(),
            await _context.Products.CountAsync());
    }
}