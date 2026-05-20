using System;

namespace Api.Dtos.Products;

public sealed class CreateProductRequest
{
    public string Name { get; init; } = default!;
    public string Sku  { get; init; } = default!;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}
