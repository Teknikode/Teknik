using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;

namespace Teknik.Areas.Users
{
    public class UserAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "User.Login", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Login",    // URL with parameters 
                 new { controller = "User", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Logout", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Logout",    // URL with parameters 
                 new { controller = "User", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Register", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Register",    // URL with parameters 
                 new { controller = "User", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Settings", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Settings",    // URL with parameters 
                 new { controller = "User", action = "Settings" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Index", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{username}",    // URL with parameters 
                 new { controller = "User", action = "Index", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.PGPKey", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{username}/PGP",    // URL with parameters 
                 new { controller = "User", action = "ViewRawPGP" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Action", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{action}",    // URL with parameters 
                 new { controller = "User", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );

            // Register Script Bundle
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/user").Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/jquery.blockUI.js",
                      "~/Scripts/bootstrap-switch.js",
                      "~/Areas/User/Scripts/User.js"));

            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/user").Include(
                      "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css"));
        }
    }
}