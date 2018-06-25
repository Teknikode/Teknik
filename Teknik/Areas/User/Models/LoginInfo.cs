using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.Areas.Users.Models
{
    public class LoginInfo
    {
        public int LoginInfoId { get; set; }

        public virtual string LoginProvider { get; set; }

        public virtual string ProviderDisplayName { get; set; }

        public virtual string ProviderKey { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
