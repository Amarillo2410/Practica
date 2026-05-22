using Application.Abstractions.Auth;

namespace Infrastructure.Services.Auth;

public sealed class ConfigurableEmailSender(
    EmailProviderResolver providerResolver,
    SmtpEmailSender smtpEmailSender,
    ResendEmailSender resendEmailSender,
    LoggingEmailSender loggingEmailSender) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        return providerResolver.ResolveProvider() switch
        {
            "smtp" => smtpEmailSender.SendAsync(to, subject, htmlBody, ct),
            "resend" => resendEmailSender.SendAsync(to, subject, htmlBody, ct),
            "log" => loggingEmailSender.SendAsync(to, subject, htmlBody, ct),
            var provider => throw new InvalidOperationException(
                $"Unsupported Email:Provider '{provider}'. Supported values: Auto, Smtp, Resend, Log.")
        };
    }
}
