using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("education");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.School).HasColumnName("school").HasMaxLength(180).IsRequired();
        builder.Property(x => x.Degree).HasColumnName("degree").HasMaxLength(150);
        builder.Property(x => x.FieldOfStudy).HasColumnName("field_of_study").HasMaxLength(150);
        builder.Property(x => x.StartYear).HasColumnName("start_year");
        builder.Property(x => x.EndYear).HasColumnName("end_year");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
    }
}
