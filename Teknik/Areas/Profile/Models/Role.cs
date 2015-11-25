using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Profile.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public PermissionType Permission { get; set; }

        public PermissionTarget Target { get; set; }
    }
}
