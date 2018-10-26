using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.IdentityServer.Models.Manage
{
    public class Enable2FAModel
    {
        public string Username { get; set; }
        public string Code { get; set; }
    }
}
