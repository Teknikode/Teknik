using System.Collections.Generic;
using System.Web.Mvc;

namespace Teknik.Areas.RSS
{
    public class RSSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "RSS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "RSS.Index", // Route name
                 new List<string>() { "dev", "rss" },
                 "",    // URL with parameters 
                 new { controller = "RSS", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.RSSController).Namespace }
             );
            context.MapSubdomainRoute(
                 "RSS.Blog", // Route name
                 new List<string>() { "dev", "rss" },
                 "Blog/{username}",    // URL with parameters 
                 new { controller = "RSS", action = "Blog", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.RSSController).Namespace }
             );
            context.MapSubdomainRoute(
                 "RSS.Podcast", // Route name
                 new List<string>() { "dev", "rss" },
                 "Podcast",    // URL with parameters 
                 new { controller = "RSS", action = "Podcast" },  // Parameter defaults 
                 new[] { typeof(Controllers.RSSController).Namespace }
             );
        }
    }
}