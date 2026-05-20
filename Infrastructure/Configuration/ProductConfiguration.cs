using System;
using Domain.Entities.Products;
using Domain.ValueObjects.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        // Definir clave primaria
        builder.HasKey(p => p.Id);

        // Definir la propiedad Name con longitud máxima
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.Sku)
            .HasConversion(
                v => v == null ? null : v.Value,
                v => string.IsNullOrWhiteSpace(v) ? null : Sku.Create(v))
            .HasColumnName("sku")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Price)
         .HasConversion(
             v => v.Amount,         
             v => new Money(v,"COP"))
         .HasColumnName("price")
         .HasColumnType("decimal(18,2)")
         .IsRequired();
    }
}
