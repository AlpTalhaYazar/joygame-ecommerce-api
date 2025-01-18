using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Domain.Entities;

public class Permission : BaseEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
}