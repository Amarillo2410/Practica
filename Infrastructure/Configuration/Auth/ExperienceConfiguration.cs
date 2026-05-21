using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("experiences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Company).HasColumnName("company").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Location).HasColumnName("location").HasMaxLength(180);
        builder.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("end_date");
        builder.Property(x => x.CurrentlyWorking).HasColumnName("currently_working").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(3000);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
    }
}
