using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teknik.IdentityServer.Models.Manage
{
    public class VerifyEmailModel
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
