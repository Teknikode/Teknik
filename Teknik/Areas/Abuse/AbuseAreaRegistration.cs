using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

namespace Teknik.Areas.Abuse
{
    public class AbuseAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Abuse";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                "Abuse.Index", // Route name
                new List<string>() { "abuse" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "",    // URL with parameters 
                new { controller = "Abuse", action = "Index" },  // Parameter defaults 
                new[] { typeof(Controllers.AbuseController).Namespace }
            );
        }
    }
}
