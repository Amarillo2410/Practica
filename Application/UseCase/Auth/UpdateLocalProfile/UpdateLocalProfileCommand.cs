using MediatR;

namespace Application.UseCase.Auth.UpdateLocalProfile;

public sealed record UpdateLocalProfileCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? Location,
    bool IsStudent,
    string? JobTitle,
    string? Company,
    string? University,
    string? Degree,
    string? Discipline,
    int? StartYear,
    string? JobSearchStatus,
    IReadOnlyCollection<string>? PreferredTitles,
    IReadOnlyCollection<string>? PreferredLocations,
    bool RemoteInterested,
    bool JobAlertsEnabled,
    bool RecruiterVisibility,
    bool CompleteOnboarding = false) : IRequest<UpdateLocalProfileResult>;
