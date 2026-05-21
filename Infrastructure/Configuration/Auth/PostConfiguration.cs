using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.AuthorId).HasColumnName("author_id").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.MediaUrls).HasColumnName("media_urls").HasColumnType("text[]");
        builder.Property(x => x.Visibility)
            .HasColumnName("visibility")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
    }
}
