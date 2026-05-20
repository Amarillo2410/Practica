using System.Net.Mail;

namespace Domain.Aggregates.Products.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El email es obligatorio.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (!MailAddress.TryCreate(normalized, out var mailAddress))
            throw new ArgumentException("El email no tiene un formato válido.", nameof(value));

        if (!string.Equals(mailAddress.Address, normalized, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El email no debe incluir nombre para mostrar u otros formatos.", nameof(value));

        return new Email(normalized);
    }

    public override string ToString() => Value;
}
