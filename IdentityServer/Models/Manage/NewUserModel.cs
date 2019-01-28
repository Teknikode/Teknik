using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Models.Manage
{
    public class NewUserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public AccountType AccountType { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public string RecoveryEmail { get; set; }
        public bool RecoveryVerified { get; set; }
        public string PGPPublicKey { get; set; }

        public NewUserModel()
        {
            AccountType = AccountType.Basic;
            AccountStatus = AccountStatus.Active;
        }
    }
}
