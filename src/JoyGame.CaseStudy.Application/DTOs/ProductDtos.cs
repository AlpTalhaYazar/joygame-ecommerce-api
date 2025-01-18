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