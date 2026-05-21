using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.Auth;

public sealed class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("user_skills");

        builder.HasKey(x => new { x.UserId, x.SkillId });

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.SkillId).HasColumnName("skill_id");
        builder.Property(x => x.EndorsementCount).HasColumnName("endorsement_count").IsRequired();

        builder.HasOne(x => x.Skill)
            .WithMany(x => x.UserSkills)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
