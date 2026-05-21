using Domain.Enums;

namespace Application.Abstractions.Auth;

public sealed class ExternalUserInfo
{
    public required AuthProvider Provider { get; init; }
    public required string ProviderUserId { get; init; }
    public required string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public required bool EmailVerified { get; init; }
}
