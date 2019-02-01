using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Utilities;

namespace Teknik.Security
{
    public class CookieEventHandler : CookieAuthenticationEvents
    {
        public CookieEventHandler()
        {
        }
        
        public override async Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = 403;
        }

        //public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        //{
        //    if (context.Principal.Identity.IsAuthenticated)
        //    {
        //        var sub = context.Principal.FindFirst("sub")?.Value;
        //        var sid = context.Principal.FindFirst("sid")?.Value;

        //        if (LogoutSessions.IsLoggedOut(sub, sid))
        //        {
        //            context.RejectPrincipal();
        //            await context.HttpContext.SignOutAsync();
        //        }
        //    }
        //}
    }
}
