namespace Domain.Aggregates.Products.ValueObjects;

public sealed record Address(string Line1, string City, string State, string Country, string ZipCode);
