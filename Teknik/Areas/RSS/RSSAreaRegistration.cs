using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

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
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "RSS.Index", // Route name
                 new List<string>() { "dev", "rss" },
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "RSS", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.RSSController).Namespace }
             );
            context.MapSubdomainRoute(
                 "RSS.Blog", // Route name
                 new List<string>() { "dev", "rss" },
                 new List<string>() { config.Host }, // domains
                 "Blog/{username}",    // URL with parameters 
                 new { controller = "RSS", action = "Blog", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.RSSController).Namespace }
             );
            context.MapSubdomainRoute(
                 "RSS.Podcast", // Route name
                 new List<string>() { "dev", "rss" },
                 new List<string>() { config.Host }, // domains
                 "Podcast",    // URL with parameters 
                 new { controller = "RSS", action = "Podcast" },  // Parameter defaults 
                 new[] { typeof(Controllers.RSSController).Namespace }
             );
        }
    }
}