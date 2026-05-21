using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class OAuthAccount : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public AuthProvider Provider { get; private set; }
    public string ProviderUserId { get; private set; } = string.Empty;
    public string ProviderEmail { get; private set; } = string.Empty;
    public string? AccessTokenEncrypted { get; private set; }
    public string? RefreshTokenEncrypted { get; private set; }
    public string? AvatarFromProvider { get; private set; }
    public DateTime LinkedAt { get; private set; }
    public User User { get; private set; } = null!;

    private OAuthAccount()
    {
    }

    public OAuthAccount(
        Guid userId,
        AuthProvider provider,
        string providerUserId,
        string providerEmail,
        string? avatarFromProvider)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(providerUserId))
        {
            throw new ArgumentException("ProviderUserId is required.", nameof(providerUserId));
        }

        if (string.IsNullOrWhiteSpace(providerEmail))
        {
            throw new ArgumentException("ProviderEmail is required.", nameof(providerEmail));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId.Trim();
        ProviderEmail = providerEmail.Trim().ToLowerInvariant();
        AvatarFromProvider = string.IsNullOrWhiteSpace(avatarFromProvider) ? null : avatarFromProvider.Trim();
        LinkedAt = DateTime.UtcNow;
    }
}
