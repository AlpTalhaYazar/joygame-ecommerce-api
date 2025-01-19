using JoyGame.CaseStudy.Domain.Enums;

namespace JoyGame.CaseStudy.Domain.Common;

public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = "System";
    public DateTime LastModifiedAt { get; set; } = DateTime.Now;
    public string LastModifiedBy { get; set; } = string.Empty;
    public EntityStatus Status { get; set; } = EntityStatus.Active;
}