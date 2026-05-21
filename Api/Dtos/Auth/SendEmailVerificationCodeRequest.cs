namespace Api.Dtos.Auth;

public sealed class SendEmailVerificationCodeRequest
{
    public Guid UserId { get; set; }
}
