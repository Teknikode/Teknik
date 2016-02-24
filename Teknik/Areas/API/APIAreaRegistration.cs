using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

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
            Config config = Config.Load();
            #region API v1
            // Base Routing
            context.MapSubdomainRoute(
                 "API.v1.Index", // Route name
                 new List<string>() { "api" },
                 new List<string>() { config.Host },
                 "v1",    // URL with parameters 
                 new { controller = "APIv1", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            // Uploads
            context.MapSubdomainRoute(
                 "API.v1.Upload", // Route name
                 new List<string>() { "api" },
                 new List<string>() { config.Host },
                 "v1/Upload",    // URL with parameters 
                 new { controller = "APIv1", action = "Upload" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            // Pastes
            context.MapSubdomainRoute(
                 "API.v1.Paste", // Route name
                 new List<string>() { "api" },
                 new List<string>() { config.Host },
                 "v1/Paste",    // URL with parameters 
                 new { controller = "APIv1", action = "Paste" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            // Url Shortening
            context.MapSubdomainRoute(
                 "API.v1.Shortener", // Route name
                 new List<string>() { "api" },
                 new List<string>() { config.Host },
                 "v1/Shorten",    // URL with parameters 
                 new { controller = "APIv1", action = "Shorten" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIv1Controller).Namespace }
             );
            #endregion

            // Default Routing
            context.MapSubdomainRoute(
                 "API.Index", // Route name
                 new List<string>() { "api" },
                 new List<string>() { config.Host },
                 "",    // URL with parameters 
                 new { controller = "API", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.APIController).Namespace }
             );
        }
    }
}