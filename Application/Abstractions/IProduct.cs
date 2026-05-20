using System;
using Domain.Entities.Products;
using Domain.ValueObjects.Products;

namespace Application.Abstractions;

public interface IProduct
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(Sku sku, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<int> CountAsync(string? search = null, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task RemoveAsync(Product product, CancellationToken ct = default);
    Task<bool> ExistsSkuAsync(Sku sku, CancellationToken ct = default);

}
