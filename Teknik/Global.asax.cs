using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Teknik.Models;
using System.Data.Entity;
using System.Web.Security;
using Teknik.Migrations;
using System.Data.Entity.Migrations;
using Teknik.Areas.Profile.Models;

namespace Teknik
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TeknikEntities, Migrations.Configuration>());

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    //let us take out the username now                
                    string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                    List<string> roles = new List<string>();

                    using (TeknikEntities entities = new TeknikEntities())
                    {
                        User user = entities.Users.Include("Groups").Include("Groups.Roles").SingleOrDefault(u => u.Username == username);

                        foreach (Group grp in user.Groups)
                        {
                            foreach (Role role in grp.Roles)
                            {
                                if (!roles.Contains(role.Name))
                                {
                                    roles.Add(role.Name);
                                }
                            }
                        }
                    }

                    //Let us set the Pricipal with our user specific details
                    HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                        new System.Security.Principal.GenericIdentity(username, "Forms"), roles.ToArray());
                }
            }
        }
    }
}
