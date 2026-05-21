using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class JobPreferencesConfiguration : IEntityTypeConfiguration<JobPreferences>
{
    public void Configure(EntityTypeBuilder<JobPreferences> builder)
    {
        builder.ToTable("job_preferences");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedNever();
        builder.Property(x => x.JobSearchStatus)
            .HasColumnName("job_search_status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(x => x.PreferredTitles).HasColumnName("preferred_titles").HasColumnType("text[]");
        builder.Property(x => x.PreferredLocations).HasColumnName("preferred_locations").HasColumnType("text[]");
        builder.Property(x => x.RemoteInterested).HasColumnName("remote_interested").IsRequired();
        builder.Property(x => x.JobAlertsEnabled).HasColumnName("job_alerts_enabled").IsRequired();
        builder.Property(x => x.RecruiterVisibility).HasColumnName("recruiter_visibility").IsRequired();
    }
}
