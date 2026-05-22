using Application.Abstractions.Auth;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public sealed class ConfigurableEmailSender(
    IOptions<EmailSenderSettings> options,
    SmtpEmailSender smtpEmailSender,
    ResendEmailSender resendEmailSender) : IEmailSender
{
    private readonly EmailSenderSettings _settings = options.Value;

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var provider = (_settings.Provider ?? "Smtp").Trim().ToLowerInvariant();

        return provider switch
        {
            "smtp" => smtpEmailSender.SendAsync(to, subject, htmlBody, ct),
            "resend" => resendEmailSender.SendAsync(to, subject, htmlBody, ct),
            _ => throw new InvalidOperationException(
                $"Unsupported Email:Provider '{_settings.Provider}'. Supported values: Smtp, Resend.")
        };
    }
}
