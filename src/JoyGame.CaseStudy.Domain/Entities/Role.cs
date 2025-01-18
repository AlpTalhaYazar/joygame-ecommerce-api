using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Domain.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();
}