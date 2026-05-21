using FluentValidation;

namespace Application.UseCase.Auth.UpdateLocalProfile;

public sealed class UpdateLocalProfileCommandValidator : AbstractValidator<UpdateLocalProfileCommand>
{
    public UpdateLocalProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

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
