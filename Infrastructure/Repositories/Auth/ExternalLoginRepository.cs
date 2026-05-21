using Application.Abstractions.Auth;
using Domain.Entities.Auth;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Auth;

public sealed class ExternalLoginRepository(AppDbContext dbContext) : IExternalLoginRepository
{
    public Task<ExternalLogin?> GetByProviderAndProviderUserIdAsync(
        AuthProvider provider,
        string providerUserId,
        CancellationToken ct = default)
    {
        var normalizedProviderUserId = providerUserId.Trim();

        return dbContext.ExternalLogins
            .AsTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.Provider == provider && x.ProviderUserId == normalizedProviderUserId,
                ct);
    }

    public Task<ExternalLogin?> GetByUserAndProviderAsync(
        Guid userId,
        AuthProvider provider,
        CancellationToken ct = default)
    {
        return dbContext.ExternalLogins
            .AsTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider, ct);
    }

    public Task AddAsync(ExternalLogin externalLogin, CancellationToken ct = default)
    {
        dbContext.ExternalLogins.Add(externalLogin);
        return Task.CompletedTask;
    }
}
