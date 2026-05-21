namespace Domain.Entities.Auth;

public sealed class RefreshToken : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;

    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.", nameof(token));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Token = token.Trim();
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
