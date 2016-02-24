using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

namespace Teknik.Areas.About
{
    public class AboutAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "About";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "About.Index", // Route name
                 new List<string>() { "about" },
                 new List<string>() { config.Host },
                 "",    // URL with parameters 
                 new { controller = "About", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.AboutController).Namespace }
             );
        }
    }
}