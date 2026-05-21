using Application.Abstractions.Auth;
using Domain.Entities.Auth;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Auth;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return dbContext.Users
            .AsTracking()
            .Include(x => x.Profile)
            .Include(x => x.ProfessionalInfo)
            .Include(x => x.JobPreferences)
            .Include(x => x.Security)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Users
            .AsTracking()
            .Include(x => x.Profile)
            .Include(x => x.ProfessionalInfo)
            .Include(x => x.JobPreferences)
            .Include(x => x.Security)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, ct);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsPublicProfileUrlAsync(string publicProfileUrl, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        var normalized = publicProfileUrl.Trim().ToLowerInvariant();
        return dbContext.UserProfiles.AnyAsync(
            x => x.PublicProfileUrl == normalized && (!excludeUserId.HasValue || x.UserId != excludeUserId.Value),
            ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
