using Application.Abstractions.Auth;
using Domain.Entities.Auth;
using Infrastructure.Context;

namespace Infrastructure.Repositories.Auth;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        dbContext.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }
}
