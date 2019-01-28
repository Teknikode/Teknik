using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Models.Manage
{
    public class UpdateAccountTypeModel
    {
        public string Username { get; set; }
        public AccountType AccountType { get; set; }
    }
}
