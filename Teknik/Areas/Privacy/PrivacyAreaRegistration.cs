using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

namespace Teknik.Areas.Privacy
{
    public class PrivacyAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Privacy";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Privacy.Index", // Route name
                 new List<string>() { "privacy" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Privacy", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PrivacyController).Namespace }
             );
        }
    }
}