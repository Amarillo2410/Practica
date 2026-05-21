using Domain.Enums;
using FluentValidation;

namespace Application.UseCase.Auth.ExternalLogin;

public sealed class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .Must(p => p is AuthProvider.Google or AuthProvider.Microsoft)
            .WithMessage("The provider is not supported.");

        RuleFor(x => x.IdToken)
            .NotEmpty()
            .MaximumLength(4096);
    }
}
