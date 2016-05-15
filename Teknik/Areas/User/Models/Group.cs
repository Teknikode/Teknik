using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class Group
    {
        public int GroupId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<User> Users { get; set; }

        public List<Role> Roles { get; set; }
    }
}
