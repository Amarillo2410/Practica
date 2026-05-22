using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Abstractions.Auth;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public sealed class ResendEmailSender(IOptions<ResendEmailSettings> options) : IEmailSender
{
    private static readonly HttpClient SharedHttpClient = new();
    private readonly ResendEmailSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("Resend email settings are not configured.");
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new InvalidOperationException("Destination email is required.");
        }

        var endpoint = $"{_settings.BaseUrl.TrimEnd('/')}/emails";
        var from = BuildFromAddress(_settings.FromEmail, _settings.FromName);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey.Trim());
        request.Content = JsonContent.Create(new ResendSendEmailRequest(
            from,
            [to.Trim()],
            subject,
            htmlBody));

        using var response = await SharedHttpClient.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException(
            $"Resend API error ({(int)response.StatusCode}): {Truncate(body, 400)}");
    }

    private static string BuildFromAddress(string fromEmail, string fromName)
    {
        var trimmedEmail = fromEmail.Trim();
        var trimmedName = fromName.Trim();
        return string.IsNullOrWhiteSpace(trimmedName)
            ? trimmedEmail
            : $"{trimmedName} <{trimmedEmail}>";
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private sealed record ResendSendEmailRequest(
        string from,
        string[] to,
        string subject,
        string html);
}
