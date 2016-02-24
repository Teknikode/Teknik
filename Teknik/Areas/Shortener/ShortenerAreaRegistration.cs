using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

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
                 new List<string>() { "dev", "shorten", "s" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Shortener", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Shortener.Action", // Route name
                 new List<string>() { "dev", "shorten", "s" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Shortener", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Shortener.View", // Route name
                 new List<string>() { "dev", "*" }, // Subdomains
                 new List<string>() { config.ShortenerConfig.ShortenerHost }, // domains
                 "{url}",    // URL with parameters 
                 new { controller = "Shortener", action = "RedirectToUrl" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
        }
    }
}