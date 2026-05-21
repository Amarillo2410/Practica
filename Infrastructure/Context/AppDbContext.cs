using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OAuthAccount> OAuthAccounts => Set<OAuthAccount>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ProfessionalInfo> ProfessionalInfo => Set<ProfessionalInfo>();
    public DbSet<JobPreferences> JobPreferences => Set<JobPreferences>();
    public DbSet<UserSecurity> UserSecurity => Set<UserSecurity>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Education> Education => Set<Education>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
