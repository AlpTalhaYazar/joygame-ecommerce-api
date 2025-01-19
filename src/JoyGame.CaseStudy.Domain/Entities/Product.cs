using JoyGame.CaseStudy.Domain.Common;
using JoyGame.CaseStudy.Domain.Enums;

namespace JoyGame.CaseStudy.Domain.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string Slug { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus BusinessStatus { get; set; } = ProductStatus.Draft;

    public required virtual Category Category { get; set; }
}