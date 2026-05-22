using Application.Abstractions.Auth;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Auth;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogWarning(
            "Email provider not configured. Verification message for {Email} with subject '{Subject}'. Body: {Body}",
            to,
            subject,
            htmlBody);

        try
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(logDirectory);
            var logPath = Path.Combine(logDirectory, "verification-emails.log");
            var entry = $"[{DateTime.UtcNow:O}] To={to} Subject={subject}{Environment.NewLine}{htmlBody}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(logPath, entry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not write verification email to dev log file.");
        }

        return Task.CompletedTask;
    }
}
