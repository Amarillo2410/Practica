using Application.Abstractions.Auth;

namespace Application.Abstractions;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IOAuthAccountRepository OAuthAccounts { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
}
