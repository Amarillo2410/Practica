using System;
using Domain.ValueObjects.Products;

namespace Domain.Entities.Products;

public sealed class Product : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Sku? Sku { get; private set; }
    public Money Price { get; private set; }
    public int Stock { get; private set; }
    

    private Product() { }
    public Product(string name, Sku sku, Money price, int stock)
    {
        Name = name;
        Sku = sku;
        Price = price;
        Stock = stock;
    }
    public void Update(string name, Sku sku, Money price, int stock)
    {
        Name = name;
        Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        Price = price;
        Stock = stock;
    }

    public void UpdatePrice(Money newPrice) => Price = newPrice;
}
