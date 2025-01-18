namespace JoyGame.CaseStudy.Domain.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public required virtual User User { get; set; }
    public required virtual Role Role { get; set; }
}