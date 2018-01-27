using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Error
{
    public class ErrorAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Error";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Error.Http404", // Route name
                 new List<string>() { "*", "error" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "404",    // URL with parameters 
                 new { controller = "Error", action = "Http404" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Error.Http403", // Route name
                 new List<string>() { "*", "error" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "403",    // URL with parameters 
                 new { controller = "Error", action = "Http403" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Error.Http500", // Route name
                 new List<string>() { "*", "error" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "500",    // URL with parameters 
                 new { controller = "Error", action = "Http500" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
            context.MapSubdomainRoute(
                "Error.Action", // Route name
                new List<string>() { "error" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "{action}",    // URL with parameters 
                new { controller = "Error", action = "Index" },  // Parameter defaults 
                new[] { typeof(Controllers.ErrorController).Namespace }
            );

            // Register Bundles
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/error", config.CdnHost).Include(
                "~/Scripts/bootbox/bootbox.min.js",
                "~/Areas/Error/Scripts/Error.js"));
        }
    }
}
