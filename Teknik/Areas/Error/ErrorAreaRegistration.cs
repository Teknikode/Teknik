using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

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
        }
    }
}