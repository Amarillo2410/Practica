namespace Domain.Entities.Auth;

public sealed class UserSecurity
{
    public Guid UserId { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }
    public User User { get; private set; } = null!;
    public ICollection<UserSession> ActiveSessions { get; private set; } = new HashSet<UserSession>();

    private UserSecurity()
    {
    }

    public UserSecurity(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        UserId = userId;
    }
}
