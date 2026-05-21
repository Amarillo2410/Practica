namespace Api.Dtos.Auth;

public sealed class SendEmailVerificationCodeResponse
{
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool AlreadyVerified { get; set; }
}
