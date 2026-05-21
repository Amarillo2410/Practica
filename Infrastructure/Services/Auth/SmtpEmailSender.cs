using System.Net;
using System.Net.Mail;
using Application.Abstractions.Auth;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public sealed class SmtpEmailSender(IOptions<SmtpEmailSettings> options) : IEmailSender
{
    private readonly SmtpEmailSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.UserName) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("SMTP email settings are not configured.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(to));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
        };

        using var registration = ct.Register(client.SendAsyncCancel);
        await client.SendMailAsync(message, ct);
    }
}
