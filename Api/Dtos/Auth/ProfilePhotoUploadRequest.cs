namespace Api.Dtos.Auth;

public sealed class ProfilePhotoUploadRequest
{
    public Guid UserId { get; set; }
    public IFormFile? File { get; set; }
}
