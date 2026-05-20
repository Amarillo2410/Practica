using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities.Auth
{
    public class RefreshToken : BaseEntity<int>
    {
        public int UserId { get; set; }
        public UserMember? UserMember { get; set; }
        public string? Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;    
    }
}