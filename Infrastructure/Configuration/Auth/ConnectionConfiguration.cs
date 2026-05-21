using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("connections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.RequesterId).HasColumnName("requester_id").IsRequired();
        builder.Property(x => x.ReceiverId).HasColumnName("receiver_id").IsRequired();
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.Requester)
            .WithMany(x => x.SentConnections)
            .HasForeignKey(x => x.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Receiver)
            .WithMany(x => x.ReceivedConnections)
            .HasForeignKey(x => x.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.RequesterId, x.ReceiverId }).IsUnique();
    }
}
