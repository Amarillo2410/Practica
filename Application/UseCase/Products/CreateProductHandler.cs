using System;
using Application.Abstractions;
using Domain.Entities.Products;
using Domain.ValueObjects.Products;
using MediatR;

namespace Application.UseCase.Products;

public sealed class CreateProductHandler: IRequestHandler<CreateProduct, Guid>
{
    private readonly IUnitOfWork uow;
    public CreateProductHandler(IUnitOfWork uow)
    {
        this.uow = uow;
    }

    public async Task<Guid> Handle(CreateProduct req, CancellationToken ct)
    {
        var sku = Sku.Create(req.Sku);
        if (await uow.Products.ExistsSkuAsync(sku, ct)) throw new InvalidOperationException("SKU duplicado");

        var product = new Product(req.Name, sku, req.Price, req.Stock);
        await uow.Products.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);
        return product.Id;
    }
}
