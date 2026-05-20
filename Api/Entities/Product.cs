using System;

namespace Api.Entities;

public sealed class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
