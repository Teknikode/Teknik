using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Teknik.Configuration;

namespace Teknik.Areas.Profile.Utility
{
    public static class UserHelper
    {
        public static HttpCookie CreateAuthCookie(string username, bool remember, string domain, bool local)
        {
            Config config = Config.Load();
            HttpCookie authcookie = FormsAuthentication.GetAuthCookie(username, remember);
            authcookie.Name = "TeknikAuth";
            authcookie.HttpOnly = true;
            authcookie.Secure = true;
            authcookie.Domain = string.Format(".{0}", domain);
            if (config.DevEnvironment)
            {
                authcookie.Domain = string.Format("dev.{0}", domain);
            }
            // Make it work for localhost
            if (local)
            {
                authcookie.Domain = domain;
                authcookie.Secure = false;
            }

            return authcookie;
        }
    }
}
