namespace Domain.Entities.Auth;

public sealed class Skill : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public ICollection<UserSkill> UserSkills { get; private set; } = new HashSet<UserSkill>();
}
