using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Teknik.Utilities
{
    public enum IdentityClientScope
    {
        [Display(Name = "openid")]
        [Description("The user identifier")]
        [ReadOnly(true)]
        openid,
        [Display(Name = "role")]
        [Description("The user role")]
        role,
        [Display(Name = "account-info")]
        [Description("A user's account information")]
        accountInfo,
        [Display(Name = "security-info")]
        [Description("A user's security information")]
        securityInfo,
        [Display(Name = "teknik-api.read")]
        [Description("Read access to the Teknik API")]
        teknikApiRead,
        [Display(Name = "teknik-api.write")]
        [Description("Write access to the Teknik API")]
        teknikApiWrite,
    }
}
