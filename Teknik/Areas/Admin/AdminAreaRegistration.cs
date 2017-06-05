using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

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
                 new { controller = "Admin", action = "Dashboard" },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Admin.Search", // Route name
                 new List<string>() { "admin" }, // Subdomains
                 new List<string>() { config.Host },
                 "Search/Users",    // URL with parameters 
                 new { controller = "Admin", action = "Search" },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Admin.UploadSearch", // Route name
                 new List<string>() { "admin" }, // Subdomains
                 new List<string>() { config.Host },
                 "Search/Uploads",    // URL with parameters 
                 new { controller = "Admin", action = "UploadSearch" },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Admin.UserInfo", // Route name
                 new List<string>() { "admin" }, // Subdomains
                 new List<string>() { config.Host },
                 "User/{username}",    // URL with parameters 
                 new { controller = "Admin", action = "UserInfo", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Admin.Action", // Route name
                 new List<string>() { "admin" }, // Subdomains
                 new List<string>() { config.Host },
                 "Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Admin", action = "Dashboard" },  // Parameter defaults 
                 new[] { typeof(Controllers.AdminController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/Search", config.CdnHost).Include(
                      "~/Areas/Admin/Scripts/Search.js"));

            // Register Script Bundles
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/UploadSearch", config.CdnHost).Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Areas/Admin/Scripts/UploadSearch.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/UserInfo", config.CdnHost).Include(
                "~/Areas/Admin/Scripts/UserInfo.js"));
        }
    }
}