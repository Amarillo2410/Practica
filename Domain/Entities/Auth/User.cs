using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class User : BaseEntity<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public OnboardingStep CurrentOnboardingStep { get; private set; } = OnboardingStep.BasicProfile;
    public bool OnboardingCompleted { get; private set; }
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = new HashSet<ExternalLogin>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new HashSet<RefreshToken>();

    private User()
    {
    }

    public User(
        string email,
        string firstName,
        string lastName,
        bool emailConfirmed,
        string? profilePictureUrl,
        OnboardingStep onboardingStep)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        EmailConfirmed = emailConfirmed;
        ProfilePictureUrl = string.IsNullOrWhiteSpace(profilePictureUrl) ? null : profilePictureUrl.Trim();
        CurrentOnboardingStep = onboardingStep;
        OnboardingCompleted = onboardingStep == OnboardingStep.Completed;
    }

    public void SetOnboardingStep(OnboardingStep step)
    {
        CurrentOnboardingStep = step;
        OnboardingCompleted = step == OnboardingStep.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfilePicture(string? profilePictureUrl)
    {
        ProfilePictureUrl = string.IsNullOrWhiteSpace(profilePictureUrl) ? null : profilePictureUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        return email.Trim().ToLowerInvariant();
    }
}
