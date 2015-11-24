using System.Web.Mvc;
using Teknik;

namespace Teknik.Areas.Home
{
    public class HomeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Home";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Home_dev", // Route name
                 "dev",
                 "Home/{controller}/{action}",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Home_subdomain", // Route name
                 "www",
                 "{controller}/{action}",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Home_default", // Route name
                 null,
                 "{controller}/{action}",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );
        }
    }
}