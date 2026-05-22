using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.Auth;

public sealed class LocalLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
