using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class OAuthAccountConfiguration : IEntityTypeConfiguration<OAuthAccount>
{
    public void Configure(EntityTypeBuilder<OAuthAccount> builder)
    {
        builder.ToTable("oauth_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ProviderEmail)
            .HasColumnName("provider_email")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AccessTokenEncrypted)
            .HasColumnName("access_token_encrypted")
            .HasMaxLength(2000);

        builder.Property(x => x.RefreshTokenEncrypted)
            .HasColumnName("refresh_token_encrypted")
            .HasMaxLength(2000);

        builder.Property(x => x.AvatarFromProvider)
            .HasColumnName("avatar_from_provider")
            .HasMaxLength(500);

        builder.Property(x => x.LinkedAt)
            .HasColumnName("linked_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique();
    }
}
