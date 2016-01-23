using System.Collections.Generic;
using System.Web.Mvc;

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
            context.MapSubdomainRoute(
                 "Error.Http404", // Route name
                 new List<string>() { "*" }, // Subdomains
                 "404",    // URL with parameters 
                 new { controller = "Error", action = "Http404" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Error.Http403", // Route name
                 new List<string>() { "*" }, // Subdomains
                 "403",    // URL with parameters 
                 new { controller = "Error", action = "Http403" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Error.Http500", // Route name
                 new List<string>() { "*" }, // Subdomains
                 "500",    // URL with parameters 
                 new { controller = "Error", action = "Http500" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
        }
    }
}