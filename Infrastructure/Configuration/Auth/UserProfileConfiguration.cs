using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever();

        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(220).IsRequired();
        builder.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        builder.Property(x => x.CoverUrl).HasColumnName("cover_url").HasMaxLength(500);
        builder.Property(x => x.Headline).HasColumnName("headline").HasMaxLength(220);
        builder.Property(x => x.About).HasColumnName("about").HasMaxLength(4000);
        builder.Property(x => x.Location).HasColumnName("location").HasMaxLength(220);
        builder.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(x => x.CurrentCompany).HasColumnName("current_company").HasMaxLength(150);
        builder.Property(x => x.CurrentPosition).HasColumnName("current_position").HasMaxLength(150);
        builder.Property(x => x.PublicProfileUrl).HasColumnName("public_profile_url").HasMaxLength(180);

        builder.HasIndex(x => x.PublicProfileUrl).IsUnique();
    }
}
