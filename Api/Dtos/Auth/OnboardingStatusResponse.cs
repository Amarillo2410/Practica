namespace Api.Dtos.Auth;

public sealed class OnboardingStatusResponse
{
    public bool Completed { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
}
