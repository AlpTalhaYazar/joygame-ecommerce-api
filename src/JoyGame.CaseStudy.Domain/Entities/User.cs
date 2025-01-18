using JoyGame.CaseStudy.Domain.Common;
using JoyGame.CaseStudy.Domain.Enums;

namespace JoyGame.CaseStudy.Domain.Entities;

public class User : BaseEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool EmailConfirmed { get; set; }
    public UserStatus BusinessStatus { get; set; } = UserStatus.Active;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
}