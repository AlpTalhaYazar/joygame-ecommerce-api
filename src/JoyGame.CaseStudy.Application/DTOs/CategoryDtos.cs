using JoyGame.CaseStudy.Domain.Entities;
using JoyGame.CaseStudy.Domain.Enums;

namespace JoyGame.CaseStudy.Application.DTOs;

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public string Slug { get; init; }
    public int? ParentId { get; init; }
    public string? ParentName { get; init; }
    public EntityStatus Status { get; init; }

    public static CategoryDto MapToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ParentId = category.ParentId,
            ParentName = category.Parent?.Name,
            Status = category.Status
        };
    }
}

public record CategoryTreeDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public string Slug { get; init; }
    public int? ParentId { get; init; }
    public List<CategoryTreeDto> Children { get; init; } = new();
}

public record CreateCategoryDto
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public int? ParentId { get; init; }
}

public record UpdateCategoryDto
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public int? ParentId { get; init; }
}

public record CategoryHierarchyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public int? ParentId { get; set; }
    public string Hierarchy { get; set; }
    public int Level { get; set; }
}