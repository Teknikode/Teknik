using System.Web.Mvc;

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
            context.MapSubdomainRoute(
                 "About_dev", // Route name
                 "dev",
                 "About/{controller}/{action}",    // URL with parameters 
                 new { area = "About", controller = "About", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.AboutController).Namespace }
             );
            context.MapSubdomainRoute(
                 "About_default", // Route name
                 "about",
                 "{controller}/{action}",    // URL with parameters 
                 new { area = this.AreaName, controller = "About", action = "Index", username = UrlParameter.Optional, page = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.AboutController).Namespace }
             );
        }
    }
}