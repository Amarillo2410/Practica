using Domain.Entities.Auth;
using Domain.Enums;

namespace Application.Abstractions.Auth;

public interface IOAuthAccountRepository
{
    Task<OAuthAccount?> GetByProviderAndProviderUserIdAsync(
        AuthProvider provider,
        string providerUserId,
        CancellationToken ct = default);

    Task<OAuthAccount?> GetByUserAndProviderAsync(
        Guid userId,
        AuthProvider provider,
        CancellationToken ct = default);

    Task AddAsync(OAuthAccount oAuthAccount, CancellationToken ct = default);
}
