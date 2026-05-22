using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class User : BaseEntity<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public OnboardingStep CurrentOnboardingStep { get; private set; } = OnboardingStep.BasicProfile;
    public bool OnboardingComplete { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public UserProfile? Profile { get; private set; }
    public ProfessionalInfo? ProfessionalInfo { get; private set; }
    public JobPreferences? JobPreferences { get; private set; }
    public UserSecurity? Security { get; private set; }
    public ICollection<OAuthAccount> OAuthAccounts { get; private set; } = new HashSet<OAuthAccount>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new HashSet<RefreshToken>();
    public ICollection<EmailVerificationCode> EmailVerificationCodes { get; private set; } = new HashSet<EmailVerificationCode>();
    public ICollection<Experience> Experiences { get; private set; } = new HashSet<Experience>();
    public ICollection<Education> Education { get; private set; } = new HashSet<Education>();
    public ICollection<UserSkill> UserSkills { get; private set; } = new HashSet<UserSkill>();
    public ICollection<Connection> SentConnections { get; private set; } = new HashSet<Connection>();
    public ICollection<Connection> ReceivedConnections { get; private set; } = new HashSet<Connection>();
    public ICollection<Post> Posts { get; private set; } = new HashSet<Post>();

    private User()
    {
    }


    public void SetOnboardingStep(OnboardingStep step)
    {
        CurrentOnboardingStep = step;
        OnboardingComplete = step == OnboardingStep.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        PasswordHash = passwordHash.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfile(UserProfile profile)
    {
        Profile = profile;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfessionalInfo(ProfessionalInfo professionalInfo)
    {
        ProfessionalInfo = professionalInfo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetJobPreferences(JobPreferences jobPreferences)
    {
        JobPreferences = jobPreferences;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSecurity(UserSecurity security)
    {
        Security = security;
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

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
