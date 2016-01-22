using System.Collections.Generic;
using System.Web.Mvc;

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
            context.MapSubdomainRoute(
                 "Privacy.Index", // Route name
                 new List<string>() { "dev", "privacy" }, // Subdomains
                 "",    // URL with parameters 
                 new { controller = "Privacy", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PrivacyController).Namespace }
             );
        }
    }
}