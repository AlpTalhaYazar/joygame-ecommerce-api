using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public int UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public virtual User User { get; set; }
}