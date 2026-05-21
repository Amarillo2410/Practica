namespace Application.UseCase.Auth.UpdateLocalProfile;

public sealed class UpdateLocalProfileResult
{
    public bool Completed { get; init; }
    public string CurrentStep { get; init; } = string.Empty;
}
