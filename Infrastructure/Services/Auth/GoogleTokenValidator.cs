using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Domain.Enums;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public sealed class GoogleTokenValidator(IOptions<GoogleAuthSettings> options) : IExternalTokenValidator
{
    private readonly GoogleAuthSettings _settings = options.Value;

    public AuthProvider Provider => AuthProvider.Google;

    public async Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new BadRequestException("The idToken field is required.");
        }

        var clientId = NormalizeClientId(_settings.ClientId);
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new BadRequestException("Authentication:Google:ClientId is not configured.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken.Trim(),
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [clientId]
                });
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedException($"Invalid Google idToken: {ex.Message}");
        }
        catch (Exception)
        {
            throw new UnauthorizedException("Invalid Google idToken.");
        }

        if (string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new UnauthorizedException("Google idToken does not contain a valid subject.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new UnauthorizedException("Google idToken does not contain an email.");
        }

        return new ExternalUserInfo
        {
            Provider = AuthProvider.Google,
            ProviderUserId = payload.Subject,
            Email = payload.Email.Trim().ToLowerInvariant(),
            FirstName = NormalizeOptional(payload.GivenName),
            LastName = NormalizeOptional(payload.FamilyName),
            ProfilePictureUrl = NormalizeOptional(payload.Picture),
            EmailVerified = payload.EmailVerified
        };
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeClientId(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return string.Empty;
        }

        var normalized = clientId.Trim();
        return normalized.StartsWith("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalized;
    }
}
