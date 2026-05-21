namespace Api.Dtos.Auth;

public sealed class ExternalLoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public AuthUserResponse User { get; set; } = new();
    public OnboardingStatusResponse Onboarding { get; set; } = new();
    public bool IsNewUser { get; set; }
}
