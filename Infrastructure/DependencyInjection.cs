using Application.Abstractions;
using Application.Abstractions.Auth;
using Infrastructure.Context;
using Infrastructure.Repositories.Auth;
using Infrastructure.Services.Auth;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
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

        services.Configure<SmtpEmailSettings>(options =>
        {
            options.Host = configuration["Email:Smtp:Host"] ?? "smtp.gmail.com";

            if (int.TryParse(configuration["Email:Smtp:Port"], out var port))
            {
                options.Port = port;
            }

            if (bool.TryParse(configuration["Email:Smtp:EnableSsl"], out var enableSsl))
            {
                options.EnableSsl = enableSsl;
            }

            options.UserName = configuration["Email:Smtp:UserName"] ?? string.Empty;
            options.Password = configuration["Email:Smtp:Password"] ?? string.Empty;
            options.FromEmail = configuration["Email:Smtp:FromEmail"] ?? string.Empty;
            options.FromName = configuration["Email:Smtp:FromName"] ?? "LinkedIn";
        });

        services.Configure<EmailSenderSettings>(options =>
        {
            options.Provider = configuration["Email:Provider"] ?? "Smtp";
        });

        services.Configure<ResendEmailSettings>(options =>
        {
            options.ApiKey = configuration["Email:Resend:ApiKey"] ?? string.Empty;
            options.BaseUrl = configuration["Email:Resend:BaseUrl"] ?? "https://api.resend.com";
            options.FromEmail = configuration["Email:Resend:FromEmail"] ?? string.Empty;
            options.FromName = configuration["Email:Resend:FromName"] ?? "LinkedIn";
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOAuthAccountRepository, OAuthAccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();
        services.AddScoped<IPasswordHashService, Pbkdf2PasswordHashService>();
        services.AddSingleton<EmailProviderResolver>();
        services.AddScoped<SmtpEmailSender>();
        services.AddScoped<ResendEmailSender>();
        services.AddScoped<LoggingEmailSender>();
        services.AddScoped<IEmailSender, ConfigurableEmailSender>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IExternalTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IExternalTokenValidator, MicrosoftTokenValidator>();

        return services;
    }
}
