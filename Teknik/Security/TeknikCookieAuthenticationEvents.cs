using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Data;

namespace Teknik.Security
{
    public class TeknikCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly TeknikEntities _dbContext;

        public TeknikCookieAuthenticationEvents(TeknikEntities dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            bool reject = false;
            User user = UserHelper.GetUser(_dbContext, context.Principal.Identity.Name);

            if (user != null)
            {
                if (user.AccountStatus == Utilities.AccountStatus.Banned)
                {
                    reject = true;
                }
            }
            else
            {
                reject = true;
            }

            if (reject)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
