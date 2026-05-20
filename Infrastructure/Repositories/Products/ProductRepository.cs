using Application.Abstractions;
using Domain.Entities.Products;
using Domain.ValueObjects.Products;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products;

public sealed class ProductRepository : IProduct
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Set<Product>().AsTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySkuAsync(Sku sku, CancellationToken ct = default) =>
        _context.Set<Product>().AsTracking().FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        _context.Set<Product>().ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Product>)t.Result, ct);

    public async Task<IReadOnlyList<Product>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        IQueryable<Product> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.Products.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.Products
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM products
                    WHERE UPPER(""Name"") LIKE {pattern}
                       OR UPPER(sku) LIKE {pattern}")
                .AsNoTracking();
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? search = null, CancellationToken ct = default)
    {
        IQueryable<Product> query;
        if (string.IsNullOrWhiteSpace(search))
        {
            query = _context.Products.AsNoTracking();
        }
        else
        {
            var pattern = $"%{search.Trim().ToUpper()}%";
            query = _context.Products
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM products
                    WHERE UPPER(""Name"") LIKE {pattern}
                       OR UPPER(sku) LIKE {pattern}")
                .AsNoTracking();
        }
        return query.CountAsync(ct);
    }
    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Product product, CancellationToken ct = default)
    {
        _context.Set<Product>().Remove(product);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsSkuAsync(Sku sku, CancellationToken ct = default) =>
        _context.Set<Product>().AnyAsync(p => p.Sku == sku, ct);
}
