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
                 "API.Index.v1", // Route name
                 "dev",
                 "API/v1",    // URL with parameters 
                 new { controller = "API", action = "Index_v1" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
            context.MapSubdomainRoute(
                 "API.Index.v1", // Route name
                 "api",
                 "v1",    // URL with parameters 
                 new { controller = "API", action = "Index_v1" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
            // Uploads
            context.MapSubdomainRoute(
                 "API.Upload.v1", // Route name
                 "dev",
                 "API/v1/Upload",    // URL with parameters 
                 new { controller = "API", action = "Upload_v1" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
            context.MapSubdomainRoute(
                 "API.Upload.v1", // Route name
                 "api",
                 "v1/Upload",    // URL with parameters 
                 new { controller = "API", action = "Upload_v1" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
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