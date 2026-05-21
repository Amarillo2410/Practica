using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class UserSecurityConfiguration : IEntityTypeConfiguration<UserSecurity>
{
    public void Configure(EntityTypeBuilder<UserSecurity> builder)
    {
        builder.ToTable("user_security");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedNever();
        builder.Property(x => x.TwoFactorEnabled).HasColumnName("two_factor_enabled").IsRequired();
        builder.Property(x => x.LastPasswordChangeAt)
            .HasColumnName("last_password_change_at")
            .HasColumnType("timestamp with time zone");
    }
}
