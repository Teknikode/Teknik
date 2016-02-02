using System.Collections.Generic;
using System.Web.Mvc;

namespace Teknik.Areas.Stream
{
    public class StreamAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Stream";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Stream.Index", // Route name
                 new List<string>() { "dev", "stream" }, // Subdomains
                 "",    // URL with parameters 
                 new { controller = "Stream", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.StreamController).Namespace }
             );
        }
    }
}