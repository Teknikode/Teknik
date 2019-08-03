using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Teknik.Utilities
{
    public enum IdentityClientGrant
    {
        [Description("Authorization Code")]
        AuthorizationCode,
        [Description("Client Credentials")]
        ClientCredentials,
        [Description("Implicit")]
        Implicit,
    }
}
