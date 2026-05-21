namespace Application.UseCase.Auth.EmailVerification;

public sealed class SendEmailVerificationCodeResult
{
    public string Email { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool AlreadyVerified { get; init; }
}
