using FluentValidation;

namespace Application.UseCase.Auth.EmailVerification;

public sealed class SendEmailVerificationCodeCommandValidator : AbstractValidator<SendEmailVerificationCodeCommand>
{
    public SendEmailVerificationCodeCommandValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                (x.UserId.HasValue && x.UserId.Value != Guid.Empty) ||
                !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Either userId or email is required.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress()
                .WithMessage("Email format is invalid.");
        });
    }
}
