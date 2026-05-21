using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class JobPreferences
{
    public Guid UserId { get; private set; }
    public JobSearchStatus JobSearchStatus { get; private set; } = JobSearchStatus.NotInterested;
    public string[] PreferredTitles { get; private set; } = [];
    public string[] PreferredLocations { get; private set; } = [];
    public bool RemoteInterested { get; private set; }
    public bool JobAlertsEnabled { get; private set; }
    public bool RecruiterVisibility { get; private set; }
    public User User { get; private set; } = null!;

    private JobPreferences()
    {
    }

    public JobPreferences(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        UserId = userId;
    }
}
