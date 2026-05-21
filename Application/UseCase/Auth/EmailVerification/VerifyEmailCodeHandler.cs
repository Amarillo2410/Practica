using Application.Abstractions;
using Application.Common.Exceptions;
using MediatR;

namespace Application.UseCase.Auth.EmailVerification;

public sealed class VerifyEmailCodeHandler(
    IUnitOfWork unitOfWork) : IRequestHandler<VerifyEmailCodeCommand, VerifyEmailCodeResult>
{
    public async Task<VerifyEmailCodeResult> Handle(VerifyEmailCodeCommand request, CancellationToken ct)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }

        if (user.IsEmailVerified)
        {
            return BuildResult(user);
        }

        var verificationCode = await unitOfWork.EmailVerificationCodes.GetLatestActiveByUserIdAsync(user.Id, ct);
        if (verificationCode is null)
        {
            throw new BadRequestException("Verification code is expired or was not requested.");
        }

        if (!string.Equals(
                verificationCode.CodeHash,
                EmailVerificationCodeHasher.Hash(request.Code),
                StringComparison.Ordinal))
        {
            verificationCode.RegisterFailedAttempt();
            await unitOfWork.SaveChangesAsync(ct);
            throw new BadRequestException("Verification code is invalid.");
        }

        verificationCode.Consume();
        user.ConfirmEmail();
        await unitOfWork.SaveChangesAsync(ct);

        return BuildResult(user);
    }

    private static VerifyEmailCodeResult BuildResult(Domain.Entities.Auth.User user)
        => new()
        {
            EmailVerified = user.IsEmailVerified,
            OnboardingCompleted = user.OnboardingComplete,
            CurrentOnboardingStep = user.CurrentOnboardingStep.ToString()
        };
}
