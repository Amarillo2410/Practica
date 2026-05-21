using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Services.Auth;

public sealed class MicrosoftTokenValidator(IOptions<MicrosoftAuthSettings> options) : IExternalTokenValidator
{
    private static readonly ConcurrentDictionary<string, IConfigurationManager<OpenIdConnectConfiguration>> ConfigurationManagers = new(StringComparer.OrdinalIgnoreCase);

    private readonly MicrosoftAuthSettings _settings = options.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public AuthProvider Provider => AuthProvider.Microsoft;

    public async Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new BadRequestException("The idToken field is required.");
        }

        var clientId = NormalizeClientId(_settings.ClientId);
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new BadRequestException("Authentication:Microsoft:ClientId is not configured.");
        }

        var tenantId = ResolveTenantId(_settings.TenantId);
        var trimmedToken = idToken.Trim();
        var tokenTenantId = ReadTenantIdClaim(trimmedToken);

        var configuration = await GetOpenIdConfigurationAsync(tenantId, ct);
        var validIssuers = BuildValidIssuers(configuration.Issuer, tenantId, tokenTenantId);

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenHandler.ValidateToken(
                trimmedToken,
                new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = configuration.SigningKeys,
                    ValidateIssuer = true,
                    ValidIssuers = validIssuers,
                    ValidateAudience = true,
                    ValidAudience = clientId,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                },
                out _);
        }
        catch (SecurityTokenException ex)
        {
            throw new UnauthorizedException($"Invalid Microsoft idToken: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            throw new UnauthorizedException($"Invalid Microsoft idToken: {ex.Message}");
        }

        var providerUserId = GetRequiredClaimValue(principal, "sub", "Microsoft idToken does not contain a valid subject.");
        var email = ResolveMicrosoftEmail(principal);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedException("Microsoft idToken does not contain a valid email.");
        }

        return new ExternalUserInfo
        {
            Provider = AuthProvider.Microsoft,
            ProviderUserId = providerUserId,
            Email = email.Trim().ToLowerInvariant(),
            FirstName = GetOptionalClaimValue(principal, "given_name"),
            LastName = GetOptionalClaimValue(principal, "family_name"),
            ProfilePictureUrl = null,
            EmailVerified = true
        };
    }

    private static string ResolveTenantId(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return "common";
        }

        var normalized = tenantId.Trim();
        return normalized.StartsWith("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase)
            ? "common"
            : normalized;
    }

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

    private static string? ReadTenantIdClaim(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return null;
        }

        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.FirstOrDefault(x => x.Type == "tid")?.Value;
    }

    private static IEnumerable<string> BuildValidIssuers(string? metadataIssuer, string tenantId, string? tokenTenantId)
    {
        var issuers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(metadataIssuer))
        {
            var trimmedMetadataIssuer = metadataIssuer.Trim();

            if (trimmedMetadataIssuer.Contains("{tenantid}", StringComparison.OrdinalIgnoreCase))
            {
                var replacementTenant = !string.IsNullOrWhiteSpace(tokenTenantId)
                    ? tokenTenantId.Trim()
                    : tenantId;

                if (!string.IsNullOrWhiteSpace(replacementTenant))
                {
                    issuers.Add(
                        trimmedMetadataIssuer.Replace(
                            "{tenantid}",
                            replacementTenant,
                            StringComparison.OrdinalIgnoreCase));
                }
            }
            else
            {
                issuers.Add(trimmedMetadataIssuer);
            }
        }

        if (string.Equals(tenantId, "common", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(tokenTenantId))
        {
            issuers.Add($"https://login.microsoftonline.com/{tokenTenantId.Trim()}/v2.0");
        }

        if (!string.Equals(tenantId, "common", StringComparison.OrdinalIgnoreCase))
        {
            issuers.Add($"https://login.microsoftonline.com/{tenantId}/v2.0");
        }

        return issuers;
    }

    private static string GetRequiredClaimValue(ClaimsPrincipal principal, string claimType, string errorMessage)
    {
        var value = GetClaimValue(principal, claimType);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new UnauthorizedException(errorMessage);
        }

        return value.Trim();
    }

    private static string ResolveMicrosoftEmail(ClaimsPrincipal principal)
    {
        var email = GetClaimValue(principal, "email");
        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        var preferredUsername = GetClaimValue(principal, "preferred_username");
        if (!string.IsNullOrWhiteSpace(preferredUsername))
        {
            return preferredUsername;
        }

        return GetClaimValue(principal, "upn") ?? string.Empty;
    }

    private static string? GetOptionalClaimValue(ClaimsPrincipal principal, string claimType)
    {
        var value = GetClaimValue(principal, claimType);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
        => principal.FindFirst(claimType)?.Value;

    private static string BuildMetadataAddress(string tenantId)
    {
        return $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
    }

    private static async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
        CancellationToken ct)
    {
        try
        {
            var configuration = await configurationManager.GetConfigurationAsync(ct);
            if (configuration.SigningKeys.Count == 0)
            {
                throw new InvalidOperationException("Microsoft OIDC metadata did not provide signing keys.");
            }

            return configuration;
        }
        catch (Exception ex) when (ex is not UnauthorizedException and not BadRequestException)
        {
            throw new InvalidOperationException("Unable to load Microsoft OIDC metadata for token validation.", ex);
        }
    }

    private Task<OpenIdConnectConfiguration> GetOpenIdConfigurationAsync(string tenantId, CancellationToken ct)
    {
        var metadataAddress = BuildMetadataAddress(tenantId);
        var configurationManager = ConfigurationManagers.GetOrAdd(
            metadataAddress,
            static address => new ConfigurationManager<OpenIdConnectConfiguration>(
                address,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever
                {
                    RequireHttps = true
                }));

        return GetConfigurationAsync(configurationManager, ct);
    }
}
