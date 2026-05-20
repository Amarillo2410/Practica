using System;
using FluentValidation;

namespace Application.UseCase.Products;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .MaximumLength(64);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");
    }
}
