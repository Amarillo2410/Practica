using System;
using Application.Abstractions;
using Application.Abstractions.Auth;
using Infrastructure.Context;
using Infrastructure.Products;
using Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _contextdb;
    private IProduct? _product;
    private IUserMemberService? _userMemberService;
    private IRolService? _rolService;
    public EfUnitOfWork(AppDbContext db)
    {
        _contextdb = db;

    }
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _contextdb.SaveChangesAsync(ct);
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        await using var tx = await _contextdb.Database.BeginTransactionAsync(ct);
        try
        {
            await operation(ct);
            await _contextdb.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
    public IProduct Products
    {
        get
        {
            _product ??= new ProductRepository(_contextdb);
            return _product;
        }
    }
    public IUserMemberService UserMembers => _userMemberService ??= new UserMemberRepository(_contextdb);
    public IRolService Roles => _rolService ??= new RolRepository(_contextdb);
    public IUserMemberRolService UserMemberRoles => throw new NotImplementedException();

}
