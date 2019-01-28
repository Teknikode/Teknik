using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Models.Manage
{
    public class UpdateAccountStatusModel
    {
        public string Username { get; set; }
        public AccountStatus AccountStatus { get; set; }
    }
}
