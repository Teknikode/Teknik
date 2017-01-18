using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Shortener
{
    public class ShortenerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Shortener";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Shortener.Index", // Route name
                 new List<string>() { "shorten", "s" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Shortener", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Shortener.Action", // Route name
                 new List<string>() { "shorten", "s" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{action}",    // URL with parameters 
                 new { controller = "Shortener", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Shortener.View", // Route name
                 new List<string>() { string.Empty, "shortened" }, // Subdomains
                 new List<string>() { config.ShortenerConfig.ShortenerHost }, // domains
                 "{url}",    // URL with parameters 
                 new { controller = "Shortener", action = "RedirectToUrl" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Shortener.Verify", // Route name
                 new List<string>() { string.Empty }, // Subdomains
                 new List<string>() { config.ShortenerConfig.ShortenerHost }, // domains
                 "",    // URL with parameters 
                 new { controller = "Shortener", action = "Verify" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/shortener", config.CdnHost).Include(
                      "~/Areas/Shortener/Scripts/Shortener.js"));
        }
    }
}