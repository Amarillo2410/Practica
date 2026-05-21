namespace Domain.Entities.Auth;

public sealed class UserProfile
{
    public Guid UserId { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? CoverUrl { get; private set; }
    public string? Headline { get; private set; }
    public string? About { get; private set; }
    public string? Location { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public string? CurrentCompany { get; private set; }
    public string? CurrentPosition { get; private set; }
    public string? PublicProfileUrl { get; private set; }
    public User User { get; private set; } = null!;

    private UserProfile()
    {
    }

    public UserProfile(Guid userId, string? firstName, string? lastName, string? avatarUrl)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        UserId = userId;
        FirstName = Normalize(firstName);
        LastName = Normalize(lastName);
        FullName = BuildFullName(FirstName, LastName);
        AvatarUrl = Normalize(avatarUrl);
        PublicProfileUrl = BuildPublicProfileUrl(FirstName, LastName);
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = Normalize(avatarUrl);
    }

    private static string BuildFullName(string? firstName, string? lastName)
        => string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? BuildPublicProfileUrl(string? firstName, string? lastName)
    {
        var fullName = BuildFullName(firstName, lastName);
        return string.IsNullOrWhiteSpace(fullName)
            ? null
            : fullName.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
