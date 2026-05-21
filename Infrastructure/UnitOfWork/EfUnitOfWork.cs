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
    private IOAuthAccountRepository? _oAuthAccounts;
    private IRefreshTokenRepository? _refreshTokens;
    private IEmailVerificationCodeRepository? _emailVerificationCodes;

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
    public IOAuthAccountRepository OAuthAccounts => _oAuthAccounts ??= new OAuthAccountRepository(_dbContext);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_dbContext);
    public IEmailVerificationCodeRepository EmailVerificationCodes =>
        _emailVerificationCodes ??= new EmailVerificationCodeRepository(_dbContext);
}
