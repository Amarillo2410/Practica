using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class Post : BaseEntity<Guid>
{
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string[] MediaUrls { get; private set; } = [];
    public PostVisibility Visibility { get; private set; } = PostVisibility.Public;
    public User Author { get; private set; } = null!;
}
