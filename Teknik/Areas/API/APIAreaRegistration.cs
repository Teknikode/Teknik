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
                 "APIv1.Index", // Route name
                 "dev",
                 "API/v1",    // URL with parameters 
                 new { controller = "APIv1", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            context.MapSubdomainRoute(
                 "APIv1.Index", // Route name
                 "api",
                 "v1",    // URL with parameters 
                 new { controller = "APIv1", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            // Uploads
            context.MapSubdomainRoute(
                 "APIv1.Upload", // Route name
                 "dev",
                 "API/v1/Upload",    // URL with parameters 
                 new { controller = "APIv1", action = "Upload" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            context.MapSubdomainRoute(
                 "APIv1.Upload", // Route name
                 "api",
                 "v1/Upload",    // URL with parameters 
                 new { controller = "APIv1", action = "Upload" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            #endregion

            // Default Routing
            context.MapSubdomainRoute(
                 "API.Index", // Route name
                 "dev",
                 "API",    // URL with parameters 
                 new { controller = "API", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
            context.MapSubdomainRoute(
                 "API.Index", // Route name
                 "api",
                 "",    // URL with parameters 
                 new { controller = "API", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
        }
    }
}