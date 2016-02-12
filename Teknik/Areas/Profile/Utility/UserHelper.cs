using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Teknik.Areas.Profile.Models;
using Teknik.Configuration;
using Teknik.Models;

namespace Teknik.Areas.Profile.Utility
{
    public static class UserHelper
    {
        public static User GetUser(string username)
        {
            TeknikEntities db = new TeknikEntities();

            User user = db.Users.Where(b => b.Username == username).FirstOrDefault();

            return user;
        }

        public static bool UserExists(string username)
        {
            User user = GetUser(username);
            if (user != null)
            {
                return true;
            }

            return false;
        }

        public static HttpCookie CreateAuthCookie(string username, bool remember, string domain, bool local)
        {
            Config config = Config.Load();
            HttpCookie authcookie = FormsAuthentication.GetAuthCookie(username, remember);
            authcookie.Name = "TeknikAuth";
            authcookie.HttpOnly = true;
            authcookie.Secure = true;

            // Set domain dependent on where it's being ran from
            if (local) // localhost
            {
                authcookie.Domain = null;
            }
            else if (config.DevEnvironment) // dev.example.com
            {
                authcookie.Domain = string.Format("dev.{0}", domain);
            }
            else // A production instance
            {
                authcookie.Domain = string.Format(".{0}", domain);
            }

            return authcookie;
        }
    }
}
