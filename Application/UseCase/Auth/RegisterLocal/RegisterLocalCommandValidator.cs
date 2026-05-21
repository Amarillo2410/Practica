using FluentValidation;

namespace Application.UseCase.Auth.RegisterLocal;

public sealed class RegisterLocalCommandValidator : AbstractValidator<RegisterLocalCommand>
{
    public RegisterLocalCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(150)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Location)
            .MaximumLength(220)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.JobTitle)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.Company)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Company));

        RuleFor(x => x.University)
            .MaximumLength(180)
            .When(x => !string.IsNullOrWhiteSpace(x.University));

        RuleFor(x => x.Degree)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Degree));

        RuleFor(x => x.Discipline)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Discipline));
    }
}
