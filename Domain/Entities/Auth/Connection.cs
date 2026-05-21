using Domain.Enums;

namespace Domain.Entities.Auth;

public sealed class Connection : BaseEntity<Guid>
{
    public Guid RequesterId { get; private set; }
    public Guid ReceiverId { get; private set; }
    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Pending;
    public User Requester { get; private set; } = null!;
    public User Receiver { get; private set; } = null!;
}
