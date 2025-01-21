using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;

namespace JoyGame.CaseStudy.Application.DTOs;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public string Slug { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; }
    public EntityStatus Status { get; init; }
    public ProductStatus BusinessStatus { get; init; }
    public int StockQuantity { get; init; }

    public static async Task<ProductDto> MapToProductDtoAsync(Product product, Category category)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? "Unknown Category",
            Status = product.Status,
            BusinessStatus = product.BusinessStatus,
            StockQuantity = product.StockQuantity
        };
    }
}

public record CreateProductDto
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public int CategoryId { get; init; }
    public int StockQuantity { get; init; }
}

public record UpdateProductDto
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public int CategoryId { get; init; }
    public int StockQuantity { get; init; }
    public ProductStatus BusinessStatus { get; init; }
}

public record ProductSearchDto
{
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public ProductStatus? Status { get; init; }
}

public record ProductWithCategoryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public string ProductSlug { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus BusinessStatus { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
    public string CategorySlug { get; set; }
}