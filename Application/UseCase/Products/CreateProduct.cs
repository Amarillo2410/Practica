
using Domain.ValueObjects.Products;
using MediatR;

namespace Application.UseCase.Products;

public sealed record CreateProduct(string Name, string Sku, Money Price, int Stock) : IRequest<Guid>;
