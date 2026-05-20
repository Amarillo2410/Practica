using System;
using FluentValidation;

namespace Application.UseCase.Products;

public sealed class CreateProductValidator : AbstractValidator<CreateProduct>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}

