using MediatR;

namespace Application.UseCase.Products;

public sealed record UpdateProduct(Guid Id, string Name, string Sku, decimal Price, int Stock) : IRequest;
