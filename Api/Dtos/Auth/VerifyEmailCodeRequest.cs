namespace Api.Dtos.Auth;

public sealed class VerifyEmailCodeRequest
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string Code { get; set; } = string.Empty;
}
