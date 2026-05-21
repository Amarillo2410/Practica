namespace Domain.Entities.Auth;

public sealed class EmailVerificationCode : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsActive => ConsumedAt is null && DateTime.UtcNow < ExpiresAt && FailedAttempts < 5;

    private EmailVerificationCode()
    {
    }

    public EmailVerificationCode(Guid userId, string codeHash, DateTime expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(codeHash))
        {
            throw new ArgumentException("Code hash is required.", nameof(codeHash));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        CodeHash = codeHash.Trim();
        ExpiresAt = expiresAt;
    }

    public void Consume()
    {
        ConsumedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterFailedAttempt()
    {
        FailedAttempts++;
        UpdatedAt = DateTime.UtcNow;
    }
}
