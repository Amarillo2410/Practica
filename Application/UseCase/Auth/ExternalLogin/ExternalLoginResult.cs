namespace Application.UseCase.Auth.ExternalLogin;

public sealed class ExternalLoginResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public ExternalLoginUserResult User { get; init; } = null!;
    public ExternalLoginOnboardingResult Onboarding { get; init; } = null!;
    public bool IsNewUser { get; init; }
    public bool VerificationCodeSent { get; init; } = true;
    public string? VerificationMessage { get; init; }
}

public sealed class ExternalLoginUserResult
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? ProfilePictureUrl { get; init; }
}

public sealed class ExternalLoginOnboardingResult
{
    public bool Completed { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
}
