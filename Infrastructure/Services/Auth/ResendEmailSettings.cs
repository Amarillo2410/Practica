namespace Infrastructure.Services.Auth;

public sealed class ResendEmailSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.resend.com";
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "LinkedIn";
}
