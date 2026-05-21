using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class ExternalLogin : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public AuthProvider Provider { get; private set; }
    public string ProviderUserId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public User User { get; private set; } = null!;

    private ExternalLogin()
    {
    }

    public ExternalLogin(Guid userId, AuthProvider provider, string providerUserId, string email)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(providerUserId))
        {
            throw new ArgumentException("ProviderUserId is required.", nameof(providerUserId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Provider = provider;
        ProviderUserId = providerUserId.Trim();
        Email = email.Trim().ToLowerInvariant();
    }
}
