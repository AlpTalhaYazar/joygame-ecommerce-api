using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; }

    public Role()
    {
        UserRoles = new HashSet<UserRole>();
        RolePermissions = new HashSet<RolePermission>();
    }
}