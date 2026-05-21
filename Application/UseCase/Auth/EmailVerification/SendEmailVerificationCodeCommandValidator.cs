using FluentValidation;

namespace Application.UseCase.Auth.EmailVerification;

public sealed class SendEmailVerificationCodeCommandValidator : AbstractValidator<SendEmailVerificationCodeCommand>
{
    public SendEmailVerificationCodeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
