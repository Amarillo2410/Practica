using Application.Abstractions;
using Application.Abstractions.Auth;
using Infrastructure.Context;
using Infrastructure.Repositories.Auth;
using Infrastructure.Services.Auth;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            string connectionString = configuration.GetConnectionString("Postgres")!;
            options.UseNpgsql(connectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.Configure<JwtOptions>(options =>
        {
            options.Key = configuration["JWT:Key"] ?? string.Empty;
            options.Issuer = configuration["JWT:Issuer"] ?? string.Empty;
            options.Audience = configuration["JWT:Audience"] ?? string.Empty;

            if (int.TryParse(configuration["JWT:DurationInMinutes"], out var durationInMinutes))
            {
                options.DurationInMinutes = durationInMinutes;
            }

            if (int.TryParse(configuration["JWT:RefreshTokenDurationInDays"], out var refreshDurationInDays))
            {
                options.RefreshTokenDurationInDays = refreshDurationInDays;
            }
        });

        services.Configure<GoogleAuthSettings>(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? string.Empty;
        });

        services.Configure<MicrosoftAuthSettings>(options =>
        {
            options.ClientId = configuration["Authentication:Microsoft:ClientId"] ?? string.Empty;
            options.TenantId = configuration["Authentication:Microsoft:TenantId"] ?? "common";
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOAuthAccountRepository, OAuthAccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IExternalTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IExternalTokenValidator, MicrosoftTokenValidator>();

        return services;
    }
}
