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
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Users
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, ct);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
