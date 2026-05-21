using FluentValidation;

namespace Application.UseCase.Auth.EmailVerification;

public sealed class VerifyEmailCodeCommandValidator : AbstractValidator<VerifyEmailCodeCommand>
{
    public VerifyEmailCodeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches("^\\d{6}$")
            .WithMessage("The verification code must contain 6 digits.");
    }
}
