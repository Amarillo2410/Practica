using System;
using Application.Abstractions;
using Domain.ValueObjects.Products;
using MediatR;

namespace Application.UseCase.Products;

public sealed class UpdateProductHandler(IUnitOfWork UoW) : IRequestHandler<UpdateProduct>
{
    public async Task Handle(UpdateProduct request, CancellationToken ct)
    {
        var product = await UoW.Products.GetByIdAsync(request.Id, ct);

        if (product is null)
            throw new KeyNotFoundException("Producto no encontrado");

        var newSku = Sku.Create(request.Sku);

        if (product.Sku != newSku &&
            await UoW.Products.ExistsSkuAsync(newSku, ct))
        {
            throw new InvalidOperationException("El SKU ya existe");
        }

        var newPrice = new Money(request.Price, "COP");

        product.Update(
            request.Name,
            newSku,
            newPrice,
            request.Stock
        );

        await UoW.Products.UpdateAsync(product, ct);
        await UoW.SaveChangesAsync(ct);
    }
}
