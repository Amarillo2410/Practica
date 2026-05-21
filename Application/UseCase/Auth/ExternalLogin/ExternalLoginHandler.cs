using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Domain.Entities.Auth;
using Domain.Enums;
using MediatR;

namespace Application.UseCase.Auth.ExternalLogin;

public sealed class ExternalLoginHandler(
    IUnitOfWork unitOfWork,
    IEnumerable<IExternalTokenValidator> tokenValidators,
    IJwtTokenService jwtTokenService) : IRequestHandler<ExternalLoginCommand, ExternalLoginResult>
{
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

        if (!externalUser.EmailVerified)
        {
            throw new UnauthorizedException($"{externalUser.Provider} account email is not verified.");
        }

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
                user = new User(
                    externalUser.Email,
                    externalUser.Provider,
                    externalUser.ProviderUserId,
                    externalUser.EmailVerified,
                    ResolveInitialOnboardingStep(externalUser));

                user.SetProfile(new UserProfile(
                    user.Id,
                    externalUser.FirstName,
                    externalUser.LastName,
                    externalUser.ProfilePictureUrl));
                user.SetProfessionalInfo(new ProfessionalInfo(user.Id));
                user.SetJobPreferences(new JobPreferences(user.Id));
                user.SetSecurity(new UserSecurity(user.Id));

                await unitOfWork.Users.AddAsync(user, ct);
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

                if (!user.IsEmailVerified && externalUser.EmailVerified)
                {
                    user.ConfirmEmail();
                }

                if (user.Profile is null)
                {
                    user.SetProfile(new UserProfile(
                        user.Id,
                        externalUser.FirstName,
                        externalUser.LastName,
                        externalUser.ProfilePictureUrl));
                }
                else if (string.IsNullOrWhiteSpace(user.Profile.AvatarUrl) &&
                    !string.IsNullOrWhiteSpace(externalUser.ProfilePictureUrl))
                {
                    user.Profile.UpdateAvatar(externalUser.ProfilePictureUrl);
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
            }
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

    private static OnboardingStep ResolveInitialOnboardingStep(ExternalUserInfo token)
    {
        var hasNames = !string.IsNullOrWhiteSpace(token.FirstName) && !string.IsNullOrWhiteSpace(token.LastName);
        return hasNames ? OnboardingStep.Location : OnboardingStep.BasicProfile;
    }
}
