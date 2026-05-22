namespace Infrastructure.Services.Auth;

public sealed class EmailSenderSettings
{
    public string Provider { get; set; } = "Smtp";
}
