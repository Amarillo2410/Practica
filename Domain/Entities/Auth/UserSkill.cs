namespace Domain.Entities.Auth;

public sealed class UserSkill
{
    public Guid UserId { get; private set; }
    public Guid SkillId { get; private set; }
    public int EndorsementCount { get; private set; }
    public User User { get; private set; } = null!;
    public Skill Skill { get; private set; } = null!;
}
