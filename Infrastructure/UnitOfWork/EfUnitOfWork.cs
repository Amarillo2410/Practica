using Application.Abstractions;
using Application.Abstractions.Auth;
using Infrastructure.Context;
using Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitOfWork;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private IUserRepository? _users;
    private IExternalLoginRepository? _externalLogins;
    private IRefreshTokenRepository? _refreshTokens;

    public EfUnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _dbContext.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            await operation(ct);
            await _dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public IUserRepository Users => _users ??= new UserRepository(_dbContext);
    public IExternalLoginRepository ExternalLogins => _externalLogins ??= new ExternalLoginRepository(_dbContext);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_dbContext);
}
