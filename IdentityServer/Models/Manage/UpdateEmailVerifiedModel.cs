using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.IdentityServer.Models.Manage
{
    public class UpdateEmailVerifiedModel
    {
        public string Username { get; set; }
        public bool Verified { get; set; }
    }
}
