using System.Web.Mvc;
using Teknik.Areas.Home.Controllers;

namespace Teknik.Areas.Dev
{
    public class DevAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Dev";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Dev_subdomain", // Route name
                 "dev",
                 "Dev/{controller}/{action}",    // URL with parameters 
                 new { controller = "Dev", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.DevController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Dev_default", // Route name
                 "dev",
                 "",    // URL with parameters 
                 "Home",
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(HomeController).Namespace }
             );
        }
    }
}