using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

namespace Teknik.Areas.Shortener
{
    public class ShortenerAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Shortener";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Stream.Index", // Route name
                 new List<string>() { "dev", "shorten", "s" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Shortener", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ShortenerController).Namespace }
             );
        }
    }
}