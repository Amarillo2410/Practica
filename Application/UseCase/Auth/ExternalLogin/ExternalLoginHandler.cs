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

        var existingExternalLogin = await unitOfWork.ExternalLogins.GetByProviderAndProviderUserIdAsync(
            externalUser.Provider,
            externalUser.ProviderUserId,
            ct);

        User user;
        var isNewUser = false;

        if (existingExternalLogin is not null)
        {
            user = existingExternalLogin.User;
        }
        else
        {
            var userByEmail = await unitOfWork.Users.GetByEmailAsync(externalUser.Email, ct);

            if (userByEmail is null)
            {
                isNewUser = true;
                user = new User(
                    externalUser.Email,
                    externalUser.FirstName ?? string.Empty,
                    externalUser.LastName ?? string.Empty,
                    externalUser.EmailVerified,
                    externalUser.ProfilePictureUrl,
                    ResolveInitialOnboardingStep(externalUser));

                await unitOfWork.Users.AddAsync(user, ct);
            }
            else
            {
                user = userByEmail;

                var existingProviderLogin = await unitOfWork.ExternalLogins.GetByUserAndProviderAsync(
                    user.Id,
                    externalUser.Provider,
                    ct);

                if (existingProviderLogin is not null &&
                    !string.Equals(existingProviderLogin.ProviderUserId, externalUser.ProviderUserId, StringComparison.Ordinal))
                {
                    throw new ConflictException(
                        $"This email is already linked to another {externalUser.Provider} account.");
                }

                if (!user.EmailConfirmed && externalUser.EmailVerified)
                {
                    user.ConfirmEmail();
                }

                if (string.IsNullOrWhiteSpace(user.ProfilePictureUrl) &&
                    !string.IsNullOrWhiteSpace(externalUser.ProfilePictureUrl))
                {
                    user.UpdateProfilePicture(externalUser.ProfilePictureUrl);
                }
            }

            var externalLogin = new Domain.Entities.Auth.ExternalLogin(
                user.Id,
                externalUser.Provider,
                externalUser.ProviderUserId,
                externalUser.Email);

            await unitOfWork.ExternalLogins.AddAsync(externalLogin, ct);
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
                FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? null : user.FirstName,
                LastName = string.IsNullOrWhiteSpace(user.LastName) ? null : user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl
            },
            Onboarding = new ExternalLoginOnboardingResult
            {
                Completed = user.OnboardingCompleted,
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
