using Mairie.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.Domain.DTOs
{
    public class UserInfoDTO
    {
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }

        public bool HasRole(RoleEnum role)
        {
            return Roles.Contains(role.ToString());
        }
    }
}
