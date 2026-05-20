using System;
using Application.Abstractions.Auth;

namespace Application.Abstractions;

public interface IUnitOfWork
{
    IProduct Products {get;}
    IUserMemberService UserMembers { get; }
    IUserMemberRolService UserMemberRoles { get; }
    IRolService Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);

}
