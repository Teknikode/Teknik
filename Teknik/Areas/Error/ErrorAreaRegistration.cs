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
                 "Error/404",    // URL with parameters 
                 new { controller = "Error", action = "Http404" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
        }
    }
}