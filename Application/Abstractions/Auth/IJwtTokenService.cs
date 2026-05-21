using Domain.Entities.Auth;

namespace Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiration();
}
