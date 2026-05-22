using FluentValidation;

namespace Application.UseCase.Auth.LocalLogin;

public sealed class LocalLoginCommandValidator : AbstractValidator<LocalLoginCommand>
{
    public LocalLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(150)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}
