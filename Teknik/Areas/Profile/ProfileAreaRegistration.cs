using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;

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
            context.MapSubdomainRoute(
                 "Profile.Login", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 "Login",    // URL with parameters 
                 new { controller = "Profile", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Logout", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 "Logout",    // URL with parameters 
                 new { controller = "Profile", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Register", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 "Register",    // URL with parameters 
                 new { controller = "Profile", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Settings", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 "Settings",    // URL with parameters 
                 new { controller = "Profile", action = "Settings" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Index", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 "{username}",    // URL with parameters 
                 new { controller = "Profile", action = "Index", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Action", // Route name
                 new List<string>() { "dev", "profile" }, // Subdomains
                 "Action/{action}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );

            // Register Script Bundle
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/profile").Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/jquery.blockUI.js",
                      "~/Areas/Profile/Scripts/Profile.js"));
        }
    }
}