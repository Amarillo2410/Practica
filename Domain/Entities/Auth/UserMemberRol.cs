using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities.Auth
{
    public class UserMemberRol
    {
        public int UserMemberId { get; set; }
        public UserMember UserMembers { get; set; } = null!;
        public int RolId { get; set; }
        public Rol Rol { get; set; }  = null!;          
    }
}