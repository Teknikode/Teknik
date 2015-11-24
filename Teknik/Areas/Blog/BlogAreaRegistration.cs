using System.Web.Mvc;

namespace Teknik.Areas.Blog
{
    public class BlogAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Blog";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapSubdomainRoute(
                 "Blog_dev", // Route name
                 "dev",
                 "Blog/{controller}/{action}",    // URL with parameters 
                 new { controller = "Blog", action = "Index" }  // Parameter defaults 
             );
            context.MapSubdomainRoute(
                 "Blog_default", // Route name
                 "blog",
                 "{controller}/{action}/{username}/{page}",    // URL with parameters 
                 new { controller = "Blog", action = "Index", username = UrlParameter.Optional, page = UrlParameter.Optional }  // Parameter defaults 
             );
        }
    }
}