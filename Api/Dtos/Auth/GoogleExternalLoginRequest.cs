using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.Auth;

public sealed class GoogleExternalLoginRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
