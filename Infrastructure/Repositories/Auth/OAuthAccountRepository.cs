using Application.Abstractions.Auth;
using Domain.Entities.Auth;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Auth;

public sealed class OAuthAccountRepository(AppDbContext dbContext) : IOAuthAccountRepository
{
    public Task<OAuthAccount?> GetByProviderAndProviderUserIdAsync(
        AuthProvider provider,
        string providerUserId,
        CancellationToken ct = default)
    {
        var normalizedProviderUserId = providerUserId.Trim();

        return dbContext.OAuthAccounts
            .AsTracking()
            .Include(x => x.User)
            .ThenInclude(x => x.Profile)
            .FirstOrDefaultAsync(
                x => x.Provider == provider && x.ProviderUserId == normalizedProviderUserId,
                ct);
    }

    public Task<OAuthAccount?> GetByUserAndProviderAsync(
        Guid userId,
        AuthProvider provider,
        CancellationToken ct = default)
    {
        return dbContext.OAuthAccounts
            .AsTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider, ct);
    }

    public Task AddAsync(OAuthAccount oAuthAccount, CancellationToken ct = default)
    {
        dbContext.OAuthAccounts.Add(oAuthAccount);
        return Task.CompletedTask;
    }
}
