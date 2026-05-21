using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.Auth;

public sealed class MicrosoftExternalLoginRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
