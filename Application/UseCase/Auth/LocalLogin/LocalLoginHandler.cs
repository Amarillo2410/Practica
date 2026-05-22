using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Application.UseCase.Auth.ExternalLogin;
using Domain.Enums;
using MediatR;

namespace Application.UseCase.Auth.LocalLogin;

public sealed class LocalLoginHandler(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService) : IRequestHandler<LocalLoginCommand, ExternalLoginResult>
{
    public async Task<ExternalLoginResult> Handle(LocalLoginCommand request, CancellationToken ct)
    {
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, ct);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.AuthProvider != AuthProvider.Local || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new UnauthorizedException("This account uses an external provider. Sign in with Google.");
        }

        if (!passwordHashService.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = jwtTokenService.GetRefreshTokenExpiration();
        var refreshToken = new Domain.Entities.Auth.RefreshToken(user.Id, refreshTokenValue, refreshTokenExpiration);
        await unitOfWork.RefreshTokens.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new ExternalLoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            IsNewUser = false,
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
            VerificationCodeSent = false,
            VerificationMessage = null
        };
    }
}
