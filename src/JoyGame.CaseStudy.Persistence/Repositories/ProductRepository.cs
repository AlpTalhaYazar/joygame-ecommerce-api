using System.Data;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<OperationResult<Product?>> GetBySlugAsync(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null)
            return OperationResult<Product?>.Failure(ErrorCode.ProductNotFound, "Product not found");

        return OperationResult<Product?>.Success(product);
    }

    public async Task<OperationResult<List<Product>>> GetByCategoryIdAsync(int categoryId)
    {
        var categoryIdsOperationResult = await GetCategoryAndChildrenIds(categoryId);

        if (!categoryIdsOperationResult.IsSuccess)
            return OperationResult<List<Product>>.Failure(categoryIdsOperationResult.ErrorCode,
                categoryIdsOperationResult.ErrorMessage);

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => categoryIdsOperationResult.Data.Contains(p.CategoryId))
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (products.Count == 0)
            return OperationResult<List<Product>>.Failure(ErrorCode.ProductNotFound, "No products found");

        return OperationResult<List<Product>>.Success(products);
    }

    public async Task<OperationResult<List<Product>>> GetActiveByCategoryIdAsync(int categoryId)
    {
        var categoryIdsOperationResult = await GetCategoryAndChildrenIds(categoryId);

        if (!categoryIdsOperationResult.IsSuccess)
            return OperationResult<List<Product>>.Failure(categoryIdsOperationResult.ErrorCode,
                categoryIdsOperationResult.ErrorMessage);

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => categoryIdsOperationResult.Data.Contains(p.CategoryId))
            .Where(p => p.Status == EntityStatus.Active)
            .Where(p => p.BusinessStatus == ProductStatus.Available)
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (products.Count == 0)
            return OperationResult<List<Product>>.Failure(ErrorCode.ProductNotFound, "No products found");

        return OperationResult<List<Product>>.Success(products);
    }

    public async Task<OperationResult<List<Product>>> SearchProductsAsync(string searchTerm, int? categoryId = null)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category);

        if (categoryId.HasValue)
        {
            var categoryIdsOperationResult = await GetCategoryAndChildrenIds(categoryId.Value);

            if (!categoryIdsOperationResult.IsSuccess)
                return OperationResult<List<Product>>.Failure(categoryIdsOperationResult.ErrorCode,
                    categoryIdsOperationResult.ErrorMessage);

            query = query.Where(p => categoryIdsOperationResult.Data.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{searchTerm}%") || (String.IsNullOrWhiteSpace(p.Description) &&
                                                                 EF.Functions.Like(p.Description, $"%{searchTerm}%")));
        }

        query = query.Where(p => p.Status == EntityStatus.Active);

        var products = await query
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (products.Count == 0)
            return OperationResult<List<Product>>.Failure(ErrorCode.ProductNotFound, "No products found");

        return OperationResult<List<Product>>.Success(products);
    }

    private async Task<OperationResult<List<int>>> GetCategoryAndChildrenIds(int categoryId)
    {
        var categories = await _context.Categories.ToListAsync();

        if (categories.Count == 0)
            return OperationResult<List<int>>.Failure(ErrorCode.CategoryNotFound, "No categories found");

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

        if (result.Count == 0)
            return OperationResult<List<int>>.Failure(ErrorCode.CategoryNotFound, "No categories found");

        return OperationResult<List<int>>.Success(result);
    }

    // Burada categoryId ile recursive olarak tüm child kategorileri
    // de arayıp döndüren bir yapı var
    public async Task<PaginatedOperationResult<(List<ProductWithCategoryDto> data, int total)>>
        GetProductsWithCategoriesAsync(
            int pageNumber = 1,
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
                ProductSlug = result.GetString(result.GetOrdinal("ProductSlug")),
                Price = result.GetDecimal(result.GetOrdinal("Price")),
                StockQuantity = result.GetInt32(result.GetOrdinal("StockQuantity")),
                BusinessStatus =
                    (ProductStatus)result.GetInt32(result.GetOrdinal("BusinessStatus")),
                CategoryId = result.GetInt32(result.GetOrdinal("CategoryId")),
                CategoryName = result.GetString(result.GetOrdinal("CategoryName")),
                CategoryDescription = result.GetString(result.GetOrdinal("CategoryDescription")),
                CategorySlug = result.GetString(result.GetOrdinal("CategorySlug"))
            });
        }

        return PaginatedOperationResult<(List<ProductWithCategoryDto> data, int total)>.Success((products, totalCount),
            new PaginatedOperationResult<(List<ProductWithCategoryDto> data, int total)>.PaginationMetadata()
            {
                Page = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
    }
}