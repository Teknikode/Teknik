using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Areas.Profile.Models
{
    public class AuthUser : IdentityUser
    {
        public User User { get; set; }
    }
}
