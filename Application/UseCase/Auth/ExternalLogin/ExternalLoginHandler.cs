using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Application.UseCase.Auth.EmailVerification;
using Domain.Entities.Auth;
using Domain.Enums;
using MediatR;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Application.UseCase.Auth.ExternalLogin;

public sealed class ExternalLoginHandler(
    IUnitOfWork unitOfWork,
    IEnumerable<IExternalTokenValidator> tokenValidators,
    IJwtTokenService jwtTokenService,
    IEmailSender emailSender,
    ILogger<ExternalLoginHandler> logger) : IRequestHandler<ExternalLoginCommand, ExternalLoginResult>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    public async Task<ExternalLoginResult> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            throw new BadRequestException("The idToken field is required.");
        }

        var tokenValidator = tokenValidators.FirstOrDefault(x => x.Provider == request.Provider);
        if (tokenValidator is null)
        {
            throw new BadRequestException($"Provider '{request.Provider}' is not configured.");
        }

        var externalUser = await tokenValidator.ValidateAsync(request.IdToken, ct);
        ValidateExternalUser(externalUser);

        var existingOAuthAccount = await unitOfWork.OAuthAccounts.GetByProviderAndProviderUserIdAsync(
            externalUser.Provider,
            externalUser.ProviderUserId,
            ct);

        User user;
        var isNewUser = false;

        if (existingOAuthAccount is not null)
        {
            user = existingOAuthAccount.User;
        }
        else
        {
            var userByEmail = await unitOfWork.Users.GetByEmailAsync(externalUser.Email, ct);

            if (userByEmail is null)
            {
                isNewUser = true;
                user = await CreateExternalUserAsync(externalUser, ct);
            }
            else
            {
                if (ShouldResetIncompleteUser(userByEmail))
                {
                    await unitOfWork.Users.DeleteAsync(userByEmail, ct);
                    await unitOfWork.SaveChangesAsync(ct);

                    isNewUser = true;
                    user = await CreateExternalUserAsync(externalUser, ct);
                }
                else
                {
                    user = userByEmail;

                    var existingProviderLogin = await unitOfWork.OAuthAccounts.GetByUserAndProviderAsync(
                        user.Id,
                        externalUser.Provider,
                        ct);

                    if (existingProviderLogin is not null &&
                        !string.Equals(existingProviderLogin.ProviderUserId, externalUser.ProviderUserId, StringComparison.Ordinal))
                    {
                        throw new ConflictException(
                            $"This email is already linked to another {externalUser.Provider} account.");
                    }

                    if (user.Profile is null)
                    {
                        var createdProfile = new UserProfile(
                            user.Id,
                            externalUser.FirstName,
                            externalUser.LastName,
                            externalUser.ProfilePictureUrl);
                        createdProfile.SetPublicProfileUrl(
                            await BuildUniquePublicProfileUrlAsync(createdProfile.PublicProfileUrl, user.Id, ct));
                        user.SetProfile(createdProfile);
                    }
                    else if (string.IsNullOrWhiteSpace(user.Profile.AvatarUrl) &&
                        !string.IsNullOrWhiteSpace(externalUser.ProfilePictureUrl))
                    {
                        user.Profile.UpdateAvatar(externalUser.ProfilePictureUrl);
                    }
                }
            }

            var oAuthAccount = new OAuthAccount(
                user.Id,
                externalUser.Provider,
                externalUser.ProviderUserId,
                externalUser.Email,
                externalUser.ProfilePictureUrl);

            await unitOfWork.OAuthAccounts.AddAsync(oAuthAccount, ct);
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = jwtTokenService.GetRefreshTokenExpiration();

        var refreshToken = new RefreshToken(user.Id, refreshTokenValue, refreshTokenExpiration);
        await unitOfWork.RefreshTokens.AddAsync(refreshToken, ct);

        await unitOfWork.SaveChangesAsync(ct);

        var verificationCodeSent = true;
        string? verificationMessage = null;

        if (!user.IsEmailVerified)
        {
            var codeValue = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var expiresAt = DateTime.UtcNow.Add(CodeLifetime);
            await unitOfWork.EmailVerificationCodes.AddAsync(
                new EmailVerificationCode(
                    user.Id,
                    EmailVerificationCodeHasher.Hash(codeValue),
                    expiresAt),
                ct);
            await unitOfWork.SaveChangesAsync(ct);

            try
            {
                await emailSender.SendAsync(
                    user.Email,
                    "Tu codigo de verificacion de LinkedIn",
                    BuildVerificationEmailBody(user.Profile?.FirstName, codeValue),
                    ct);
            }
            catch (Exception ex)
            {
                verificationCodeSent = false;
                verificationMessage = "No se pudo enviar el codigo de verificacion. Usa 'Reenviar codigo' para intentarlo de nuevo.";
                logger.LogWarning(ex, "Verification code email could not be sent for user {UserId}", user.Id);
            }
        }

        return new ExternalLoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            IsNewUser = isNewUser,
            User = new ExternalLoginUserResult
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = string.IsNullOrWhiteSpace(user.Profile?.FirstName) ? null : user.Profile.FirstName,
                LastName = string.IsNullOrWhiteSpace(user.Profile?.LastName) ? null : user.Profile.LastName,
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

    private static void ValidateExternalUser(ExternalUserInfo externalUser)
    {
        if (externalUser.Provider == default)
        {
            throw new UnauthorizedException("External provider is invalid.");
        }

        if (string.IsNullOrWhiteSpace(externalUser.ProviderUserId))
        {
            throw new UnauthorizedException("External provider user id is missing.");
        }

        if (string.IsNullOrWhiteSpace(externalUser.Email))
        {
            throw new UnauthorizedException("External provider email is missing.");
        }
    }

    private static bool ShouldResetIncompleteUser(User user)
        => !user.IsEmailVerified && !user.OnboardingComplete;

    private static OnboardingStep ResolveInitialOnboardingStep(ExternalUserInfo token)
        => OnboardingStep.BasicProfile;

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

    private async Task<User> CreateExternalUserAsync(ExternalUserInfo externalUser, CancellationToken ct)
    {

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
