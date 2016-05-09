using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

namespace Teknik.Areas.TOS
{
    public class TOSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TOS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "TOS.Index", // Route name
                 new List<string>() { "tos" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "TOS", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.TOSController).Namespace }
             );
        }
    }
}