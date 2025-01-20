using System.Data;
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

    // Burada categoryId ile recursive olarak tüm child kategorileri
    // de arayıp döndüren bir yapı var
    public async Task<(List<ProductWithCategoryDto> data, int total)> GetProductsWithCategoriesAsync(int pageNumber = 1,
        int pageSize = 10, int? categoryId = null, string? searchText = null)
    {
        var parameters = new[]
        {
            new SqlParameter("@PageNumber", pageNumber),
            new SqlParameter("@PageSize", pageSize),
            new SqlParameter("@CategoryId", (object)categoryId ?? DBNull.Value),
            new SqlParameter("@SearchText", (object)searchText ?? DBNull.Value)
        };

        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "GetProductsWithCategories";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddRange(parameters);

        await _context.Database.OpenConnectionAsync();

        using var result = await command.ExecuteReaderAsync();

        // İlk command ile toplam sayıyı alıyoruz
        await result.ReadAsync();
        var totalCount = result.GetInt32(0);

        // İkinci commanda geçiyoruz
        await result.NextResultAsync();

        var products = new List<ProductWithCategoryDto>();
        while (await result.ReadAsync())
        {
            products.Add(new ProductWithCategoryDto
            {
                ProductId = result.GetInt32(result.GetOrdinal("ProductId")),
                ProductName = result.GetString(result.GetOrdinal("ProductName")),
                ProductDescription = result.GetString(result.GetOrdinal("ProductDescription")),
                Price = result.GetDecimal(result.GetOrdinal("Price")),
                StockQuantity = result.GetInt32(result.GetOrdinal("StockQuantity")),
                BusinessStatus =
                    (ProductStatus)result.GetInt32(result.GetOrdinal("BusinessStatus")), // Changed this line
                CategoryId = result.GetInt32(result.GetOrdinal("CategoryId")),
                CategoryName = result.GetString(result.GetOrdinal("CategoryName")),
                CategoryDescription = result.GetString(result.GetOrdinal("CategoryDescription"))
            });
        }

        return (products, totalCount);
    }
}