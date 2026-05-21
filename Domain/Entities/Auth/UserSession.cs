namespace Domain.Entities.Auth;

public sealed class UserSession : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public string Device { get; private set; } = string.Empty;
    public string? Ip { get; private set; }
    public string? Location { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public User User { get; private set; } = null!;
    public UserSecurity Security { get; private set; } = null!;

    private UserSession()
    {
    }

    public UserSession(Guid userId, string device, string? ip, string? location)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(device))
        {
            throw new ArgumentException("Device is required.", nameof(device));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Device = device.Trim();
        Ip = string.IsNullOrWhiteSpace(ip) ? null : ip.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        LastSeenAt = DateTime.UtcNow;
    }
}
