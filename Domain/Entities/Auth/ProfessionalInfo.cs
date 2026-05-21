namespace Domain.Entities.Auth;

public sealed class ProfessionalInfo
{
    public Guid UserId { get; private set; }
    public bool IsStudent { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Company { get; private set; }
    public string? University { get; private set; }
    public string? Degree { get; private set; }
    public string? Discipline { get; private set; }
    public int? StartYear { get; private set; }
    public string[] Skills { get; private set; } = [];
    public string[] Interests { get; private set; } = [];
    public User User { get; private set; } = null!;

    private ProfessionalInfo()
    {
    }

    public ProfessionalInfo(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        UserId = userId;
    }

    public void UpdateExperience(
        bool isStudent,
        string? jobTitle,
        string? company,
        string? university,
        string? degree,
        string? discipline,
        int? startYear)
    {
        IsStudent = isStudent;
        JobTitle = Normalize(jobTitle);
        Company = Normalize(company);
        University = Normalize(university);
        Degree = Normalize(degree);
        Discipline = Normalize(discipline);
        StartYear = startYear;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
