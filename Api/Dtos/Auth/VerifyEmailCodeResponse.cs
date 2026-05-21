namespace Api.Dtos.Auth;

public sealed class VerifyEmailCodeResponse
{
    public bool EmailVerified { get; set; }
    public OnboardingStatusResponse Onboarding { get; set; } = new();
}
