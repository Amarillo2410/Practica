using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities.Auth;
using Domain.Enums;
using MediatR;

namespace Application.UseCase.Auth.UpdateLocalProfile;

public sealed class UpdateLocalProfileHandler(
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateLocalProfileCommand, UpdateLocalProfileResult>
{
    public async Task<UpdateLocalProfileResult> Handle(UpdateLocalProfileCommand request, CancellationToken ct)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }

        var profile = user.Profile ?? new UserProfile(user.Id, firstName: null, lastName: null, avatarUrl: null);
        var professionalInfo = user.ProfessionalInfo ?? new ProfessionalInfo(user.Id);
        var jobPreferences = user.JobPreferences ?? new JobPreferences(user.Id);

        var firstName = string.IsNullOrWhiteSpace(request.FirstName)
            ? profile.FirstName
            : request.FirstName;

        var lastName = string.IsNullOrWhiteSpace(request.LastName)
            ? profile.LastName
            : request.LastName;

        profile.UpdateBasicInfo(firstName, lastName);
        profile.SetPublicProfileUrl(
            await BuildUniquePublicProfileUrlAsync(profile.PublicProfileUrl, user.Id, ct));

        professionalInfo.UpdateExperience(
            request.IsStudent,
            request.JobTitle,
            request.Company,
            request.University,
            request.Degree,
            request.Discipline,
            request.StartYear);

        var headline = !string.IsNullOrWhiteSpace(request.JobTitle)
            ? request.JobTitle
            : request.IsStudent
                ? request.Degree
                : null;

        profile.UpdateOnboardingDetails(
            request.Location,
            headline,
            request.Company,
            request.JobTitle);

        var jobSearchStatus = ParseJobSearchStatus(request.JobSearchStatus);
        jobPreferences.UpdatePreferences(
            jobSearchStatus,
            request.PreferredTitles,
            request.PreferredLocations,
            request.RemoteInterested,
            request.JobAlertsEnabled,
            request.RecruiterVisibility);

        user.SetProfile(profile);
        user.SetProfessionalInfo(professionalInfo);
        user.SetJobPreferences(jobPreferences);

        user.SetOnboardingStep(ResolveOnboardingStep(request, jobSearchStatus, user.IsEmailVerified));

        await unitOfWork.SaveChangesAsync(ct);

        return new UpdateLocalProfileResult
        {
            Completed = user.OnboardingComplete,
            CurrentStep = user.CurrentOnboardingStep.ToString()
        };
    }

    private async Task<string> BuildUniquePublicProfileUrlAsync(
        string? baseSlug,
        Guid userId,
        CancellationToken ct)
    {
        var seed = string.IsNullOrWhiteSpace(baseSlug)
            ? $"user-{userId:N}".ToLowerInvariant()
            : baseSlug.Trim().ToLowerInvariant();

        var candidate = seed;
        var suffix = 1;

        while (await unitOfWork.Users.ExistsPublicProfileUrlAsync(candidate, excludeUserId: userId, ct))
        {
            candidate = $"{seed}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static JobSearchStatus ParseJobSearchStatus(string? value)
    {
        if (Enum.TryParse<JobSearchStatus>(value, ignoreCase: true, out var parsedStatus))
        {
            return parsedStatus;
        }

        return JobSearchStatus.NotInterested;
    }

    private static OnboardingStep ResolveOnboardingStep(
        UpdateLocalProfileCommand request,
        JobSearchStatus jobSearchStatus,
        bool isEmailVerified)
    {
        if (string.IsNullOrWhiteSpace(request.Location))
        {
            return OnboardingStep.Location;
        }

        var hasExperienceInfo = request.IsStudent
            ? !string.IsNullOrWhiteSpace(request.University) || !string.IsNullOrWhiteSpace(request.Discipline)
            : !string.IsNullOrWhiteSpace(request.JobTitle) || !string.IsNullOrWhiteSpace(request.Company);

        if (!hasExperienceInfo)
        {
            return OnboardingStep.Experience;
        }

        var hasJobPreferences = jobSearchStatus == JobSearchStatus.NotInterested ||
            (request.PreferredTitles?.Any() == true && request.PreferredLocations?.Any() == true);

        if (!hasJobPreferences)
        {
            return OnboardingStep.JobPreferences;
        }

        if (!isEmailVerified)
        {
            return OnboardingStep.PhoneVerification;
        }

        return OnboardingStep.Completed;
    }
}
