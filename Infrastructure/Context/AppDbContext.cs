using System;
using Domain.Entities.Auth;
using Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public sealed  class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<UserMember> UsersMembers => Set<UserMember>();
    public DbSet<Rol> Rols => Set<Rol>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
