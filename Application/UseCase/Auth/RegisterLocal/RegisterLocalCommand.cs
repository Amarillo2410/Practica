using Application.UseCase.Auth.ExternalLogin;
using MediatR;

namespace Application.UseCase.Auth.RegisterLocal;

public sealed record RegisterLocalCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
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
    bool RecruiterVisibility) : IRequest<ExternalLoginResult>;
