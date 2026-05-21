namespace Domain.Entities.Auth;

public sealed class Experience : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Company { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool CurrentlyWorking { get; private set; }
    public string? Description { get; private set; }
    public User User { get; private set; } = null!;
}
