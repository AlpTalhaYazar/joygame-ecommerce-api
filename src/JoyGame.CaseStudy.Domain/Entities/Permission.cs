using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; }

    public Permission()
    {
        RolePermissions = new HashSet<RolePermission>();
    }
}