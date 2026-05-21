namespace Application.UseCase.Auth.EmailVerification;

public sealed class VerifyEmailCodeResult
{
    public bool EmailVerified { get; init; }
    public bool OnboardingCompleted { get; init; }
    public string CurrentOnboardingStep { get; init; } = string.Empty;
}
