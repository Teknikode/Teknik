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
                 "Privacy_dev", // Route name
                 "dev",
                 "Privacy/{controller}/{action}",    // URL with parameters 
                 new { controller = "Privacy", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PrivacyController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Privacy_default", // Route name
                 "privacy",
                 "{controller}/{action}",    // URL with parameters 
                 new { controller = "Privacy", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PrivacyController).Namespace }
             );
        }
    }
}