using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Teknik.Utilities
{
    public enum IdentityClientScope
    {
        [DisplayName("openid")]
        [Description("The user identifier")]
        [ReadOnly(true)]
        openid,
        [DisplayName("role")]
        [Description("The user role")]
        role,
        [DisplayName("account-info")]
        [Description("A user's account information")]
        accountInfo,
        [DisplayName("security-info")]
        [Description("A user's security information")]
        securityInfo,
        [DisplayName("teknik-api.read")]
        [Description("Read access to the Teknik API")]
        teknikApiRead,
        [DisplayName("teknik-api.write")]
        [Description("Write access to the Teknik API")]
        teknikApiWrite,
    }
}
