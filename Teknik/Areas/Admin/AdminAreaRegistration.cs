using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Helpers;

namespace Teknik.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Admin.Dashboard", // Route name
                 new List<string>() { "admin" }, // Subdomains
                 new List<string>() { config.Host },
                 "",    // URL with parameters 
                 new { controller = "Admin", action = "Dashboard", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Admin.Search", // Route name
                 new List<string>() { "admin" }, // Subdomains
                 new List<string>() { config.Host },
                 "Search",    // URL with parameters 
                 new { controller = "Admin", action = "Search", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Admin.Action", // Route name
                 new List<string>() { "blog" }, // Subdomains
                 new List<string>() { config.Host },
                 "Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Admin", action = "Dashboard" },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/Search", config.CdnHost).Include(
                      "~/Areas/Admin/Scripts/Search.js"));
        }
    }
}