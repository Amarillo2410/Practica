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

    public void UpdatePreferences(
        JobSearchStatus jobSearchStatus,
        IEnumerable<string>? preferredTitles,
        IEnumerable<string>? preferredLocations,
        bool remoteInterested,
        bool jobAlertsEnabled,
        bool recruiterVisibility)
    {
        JobSearchStatus = jobSearchStatus;
        PreferredTitles = NormalizeList(preferredTitles);
        PreferredLocations = NormalizeList(preferredLocations);
        RemoteInterested = remoteInterested;
        JobAlertsEnabled = jobAlertsEnabled;
        RecruiterVisibility = recruiterVisibility;
    }

    private static string[] NormalizeList(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        return values
            .Select(x => x?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
