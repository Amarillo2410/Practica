using System;

namespace Domain.ValueObjects.Products;

public class Sku : IEquatable<Sku>
{
    public string Value { get; }
    private Sku(string value) => Value = value.ToUpperInvariant();

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("SKU vacío");
        return new Sku(value.Trim());
    }
    public bool Equals(Sku? other) =>
        other is not null && StringComparer.Ordinal.Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is Sku o && Equals(o);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    // (Opcional) operadores
    public static bool operator ==(Sku? a, Sku? b) => Equals(a, b);
    public static bool operator !=(Sku? a, Sku? b) => !Equals(a, b);
}
