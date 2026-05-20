using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities.Auth
{
    public class UserMember : BaseEntity<int>
    {
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ICollection<Rol> Rols { get; set; } = new HashSet<Rol>();
    public ICollection<UserMemberRol> UserMemberRols { get; set; } = new HashSet<UserMemberRol>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();

    }
}