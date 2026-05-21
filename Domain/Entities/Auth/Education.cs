namespace Domain.Entities.Auth;

public sealed class Education : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public string School { get; private set; } = string.Empty;
    public string? Degree { get; private set; }
    public string? FieldOfStudy { get; private set; }
    public int? StartYear { get; private set; }
    public int? EndYear { get; private set; }
    public User User { get; private set; } = null!;
}
