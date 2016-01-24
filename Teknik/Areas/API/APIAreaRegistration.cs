using System.Collections.Generic;
using System.Web.Mvc;

namespace Teknik.Areas.API
{
    public class APIAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "API";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            #region API v1
            // Base Routing
            context.MapSubdomainRoute(
                 "API.v1.Index", // Route name
                 new List<string>() { "dev", "api" },
                 "v1",    // URL with parameters 
                 new { controller = "APIv1", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            // Uploads
            context.MapSubdomainRoute(
                 "API.v1.Upload", // Route name
                 new List<string>() { "dev", "api" },
                 "v1/Upload",    // URL with parameters 
                 new { controller = "APIv1", action = "Upload" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            context.MapSubdomainRoute(
                 "API.v1.Paste", // Route name
                 new List<string>() { "dev", "api" },
                 "v1/Paste",    // URL with parameters 
                 new { controller = "APIv1", action = "Paste" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            #endregion

            // Default Routing
            context.MapSubdomainRoute(
                 "API.Index", // Route name
                 new List<string>() { "dev", "" },
                 "",    // URL with parameters 
                 new { controller = "API", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
        }
    }
}