using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;

namespace Teknik.Areas.Profile
{
    public class ProfileAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Profile";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Profile.Login", // Route name
                 new List<string>() { "dev", "profile", "www", string.Empty }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Login",    // URL with parameters 
                 new { controller = "Profile", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Logout", // Route name
                 new List<string>() { "dev", "profile", "www", string.Empty }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Logout",    // URL with parameters 
                 new { controller = "Profile", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Register", // Route name
                 new List<string>() { "dev", "profile", "www", string.Empty }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Register",    // URL with parameters 
                 new { controller = "Profile", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Settings", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Settings",    // URL with parameters 
                 new { controller = "Profile", action = "Settings" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Index", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{username}",    // URL with parameters 
                 new { controller = "Profile", action = "Index", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Action", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{action}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );

            // Register Script Bundle
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/profile").Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/jquery.blockUI.js",
                      "~/Scripts/bootstrap-switch.js",
                      "~/Areas/Profile/Scripts/Profile.js"));

            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/profile").Include(
                      "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css"));
        }
    }
}