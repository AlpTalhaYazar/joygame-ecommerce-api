using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string Slug { get; set; }
    public int ParentId { get; set; }

    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; } = new HashSet<Category>();
    public virtual ICollection<Product> Products { get; set; } = new HashSet<Product>();
}