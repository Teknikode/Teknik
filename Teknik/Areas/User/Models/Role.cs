using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
