using System.Web.Mvc;

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
            //Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Dev_subdomain", // Route name
                 "dev",
                 "{area}/{controller}/{action}",    // URL with parameters 
                 new { area = "Home", controller = "Home", action = "Index" }  // Parameter defaults 
             );
            //context.MapRoute(
            //    "Dev_default",
            //    "Dev/{controller}/{action}",
            //    new { controller = "Dev", action = "Index" },
            //    namespaces: new[] { "Teknik.Areas.Dev.Controllers" }
            //);
        }
    }
}