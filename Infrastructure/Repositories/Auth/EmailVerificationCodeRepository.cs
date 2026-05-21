using Application.Abstractions.Auth;
using Domain.Entities.Auth;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Auth;

public sealed class EmailVerificationCodeRepository(AppDbContext dbContext) : IEmailVerificationCodeRepository
{
    public Task<EmailVerificationCode?> GetLatestActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        return dbContext.EmailVerificationCodes
            .AsTracking()
            .Where(x => x.UserId == userId &&
                x.ConsumedAt == null &&
                x.ExpiresAt > now &&
                x.FailedAttempts < 5)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public Task AddAsync(EmailVerificationCode code, CancellationToken ct = default)
    {
        dbContext.EmailVerificationCodes.Add(code);
        return Task.CompletedTask;
    }
}
