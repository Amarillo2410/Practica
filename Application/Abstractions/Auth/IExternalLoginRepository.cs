using Domain.Entities.Auth;
using Domain.Enums;

namespace Application.Abstractions.Auth;

public interface IExternalLoginRepository
{
    Task<ExternalLogin?> GetByProviderAndProviderUserIdAsync(
        AuthProvider provider,
        string providerUserId,
        CancellationToken ct = default);

    Task<ExternalLogin?> GetByUserAndProviderAsync(
        Guid userId,
        AuthProvider provider,
        CancellationToken ct = default);

    Task AddAsync(ExternalLogin externalLogin, CancellationToken ct = default);
}
