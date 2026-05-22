using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Application.UseCase.Auth.EmailVerification;
using Application.UseCase.Auth.ExternalLogin;
using Domain.Entities.Auth;
using Domain.Enums;
using MediatR;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Application.UseCase.Auth.RegisterLocal;

public sealed class RegisterLocalHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    IEmailSender emailSender,
    ILogger<RegisterLocalHandler> logger) : IRequestHandler<RegisterLocalCommand, ExternalLoginResult>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    public async Task<ExternalLoginResult> Handle(RegisterLocalCommand request, CancellationToken ct)
    {
        var existingUser = await unitOfWork.Users.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var jobSearchStatus = ParseJobSearchStatus(request.JobSearchStatus);
        var onboardingStep = ResolveOnboardingStep(request, jobSearchStatus, isEmailVerified: false);

        var user = new User(
            request.Email,
            AuthProvider.Local,
            providerId: null,
            isEmailVerified: false,
            onboardingStep);

        user.SetPasswordHash(passwordHashService.Hash(request.Password));

        var profile = new UserProfile(
            user.Id,
            request.FirstName,
            request.LastName,
            avatarUrl: null);

        profile.SetPublicProfileUrl(
            await BuildUniquePublicProfileUrlAsync(profile.PublicProfileUrl, user.Id, ct));

        profile.UpdateOnboardingDetails(
            request.Location,
            BuildHeadline(request),
            request.Company,
            request.JobTitle);

        var professionalInfo = new ProfessionalInfo(user.Id);
        professionalInfo.UpdateExperience(
            request.IsStudent,
            request.JobTitle,
            request.Company,
            request.University,
            request.Degree,
            request.Discipline,
            request.StartYear);

        var jobPreferences = new JobPreferences(user.Id);
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
        user.SetSecurity(new UserSecurity(user.Id));

        await unitOfWork.Users.AddAsync(user, ct);

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = jwtTokenService.GetRefreshTokenExpiration();

        var refreshToken = new RefreshToken(user.Id, refreshTokenValue, refreshTokenExpiration);
        await unitOfWork.RefreshTokens.AddAsync(refreshToken, ct);

        var verificationCodeValue = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var verificationExpiresAt = DateTime.UtcNow.Add(CodeLifetime);
        await unitOfWork.EmailVerificationCodes.AddAsync(
            new EmailVerificationCode(
                user.Id,
                EmailVerificationCodeHasher.Hash(verificationCodeValue),
                verificationExpiresAt),
            ct);

        await unitOfWork.SaveChangesAsync(ct);

        var verificationCodeSent = true;
        string? verificationMessage = null;

        try
        {
            await emailSender.SendAsync(
                user.Email,
                "Tu codigo de verificacion de LinkedIn",
                BuildVerificationEmailBody(user.Profile?.FirstName, verificationCodeValue),
                ct);
        }
        catch (Exception ex)
        {
            verificationCodeSent = false;
            verificationMessage = "No se pudo enviar el codigo de verificacion. Usa 'Reenviar codigo' para intentarlo de nuevo.";
            logger.LogWarning(ex, "Verification code email could not be sent for user {UserId}", user.Id);
        }

        return new ExternalLoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            IsNewUser = true,
            User = new ExternalLoginUserResult
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.Profile?.FirstName,
                LastName = user.Profile?.LastName,
                ProfilePictureUrl = user.Profile?.AvatarUrl
            },
            Onboarding = new ExternalLoginOnboardingResult
            {
                Completed = user.OnboardingComplete,
                CurrentStep = user.CurrentOnboardingStep.ToString()
            },
            VerificationCodeSent = verificationCodeSent,
            VerificationMessage = verificationMessage
        };
    }

    private static string? BuildHeadline(RegisterLocalCommand request)
    {
        if (!string.IsNullOrWhiteSpace(request.JobTitle))
        {
            return request.JobTitle;
        }

        return request.IsStudent ? request.Degree : null;
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
        RegisterLocalCommand request,
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

    private static string BuildVerificationEmailBody(string? firstName, string code)
    {
        var safeName = string.IsNullOrWhiteSpace(firstName) ? "Hola" : firstName.Trim();
        return $"""
            <div style="font-family:Arial,sans-serif;line-height:1.5;color:#1f2328">
              <h2>{safeName}, verifica tu email</h2>
              <p>Usa este codigo para completar tu registro:</p>
              <p style="font-size:28px;font-weight:700;letter-spacing:6px">{code}</p>
              <p>El codigo vence en 10 minutos.</p>
            </div>
            """;
    }
}
