using Domain.Entities.Auth;

namespace Application.Abstractions.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
}
