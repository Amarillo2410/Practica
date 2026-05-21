using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class ProfessionalInfoConfiguration : IEntityTypeConfiguration<ProfessionalInfo>
{
    public void Configure(EntityTypeBuilder<ProfessionalInfo> builder)
    {
        builder.ToTable("professional_info");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedNever();
        builder.Property(x => x.IsStudent).HasColumnName("is_student").IsRequired();
        builder.Property(x => x.JobTitle).HasColumnName("job_title").HasMaxLength(150);
        builder.Property(x => x.Company).HasColumnName("company").HasMaxLength(150);
        builder.Property(x => x.University).HasColumnName("university").HasMaxLength(180);
        builder.Property(x => x.Degree).HasColumnName("degree").HasMaxLength(150);
        builder.Property(x => x.Discipline).HasColumnName("discipline").HasMaxLength(150);
        builder.Property(x => x.StartYear).HasColumnName("start_year");
        builder.Property(x => x.Skills).HasColumnName("skills").HasColumnType("text[]");
        builder.Property(x => x.Interests).HasColumnName("interests").HasColumnType("text[]");
    }
}
