using Domain.Enums;

namespace Application.Abstractions.Auth;

public interface IExternalTokenValidator
{
    AuthProvider Provider { get; }
    Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken ct = default);
}
