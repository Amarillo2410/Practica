using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public sealed class EmailProviderResolver(
    IOptions<EmailSenderSettings> senderSettings,
    IOptions<SmtpEmailSettings> smtpSettings,
    IOptions<ResendEmailSettings> resendSettings)
{
    public string ResolveProvider()
    {
        var configured = (senderSettings.Value.Provider ?? "Auto").Trim();
        if (!configured.Equals("Auto", StringComparison.OrdinalIgnoreCase))
        {
            return configured.ToLowerInvariant();
        }

        if (IsResendConfigured())
        {
            return "resend";
        }

        if (IsSmtpConfigured())
        {
            return "smtp";
        }

        return "log";
    }

    public bool IsResendConfigured()
        => !string.IsNullOrWhiteSpace(resendSettings.Value.ApiKey)
           && !string.IsNullOrWhiteSpace(resendSettings.Value.FromEmail);

    public bool IsSmtpConfigured()
        => !string.IsNullOrWhiteSpace(smtpSettings.Value.UserName)
           && !string.IsNullOrWhiteSpace(smtpSettings.Value.Password)
           && !string.IsNullOrWhiteSpace(smtpSettings.Value.FromEmail);
}
