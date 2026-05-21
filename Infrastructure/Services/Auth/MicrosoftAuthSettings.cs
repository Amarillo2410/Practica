namespace Infrastructure.Services.Auth;

public sealed class MicrosoftAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = "common";
}
