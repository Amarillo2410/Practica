namespace Api.Dtos.Auth;

public sealed class LocalProfileUpdateRequest
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public bool IsStudent { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? University { get; set; }
    public string? Degree { get; set; }
    public string? Discipline { get; set; }
    public int? StartYear { get; set; }
    public string? JobSearchStatus { get; set; }
    public string[] PreferredTitles { get; set; } = [];
    public string[] PreferredLocations { get; set; } = [];
    public bool RemoteInterested { get; set; }
    public bool JobAlertsEnabled { get; set; } = true;
    public bool RecruiterVisibility { get; set; } = true;
}
