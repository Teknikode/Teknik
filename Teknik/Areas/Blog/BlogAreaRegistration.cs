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
            //context.MapSubdomainRoute(
            //     "Blog_dev", // Route name
            //     "dev",
            //     "blog/{controller}/{action}/{username}/{page}",    // URL with parameters 
            //     new { subdomain = "blog", area = this.AreaName, controller = "Blog", action = "Index", username = UrlParameter.Optional, page = UrlParameter.Optional }  // Parameter defaults 
            // );
            context.MapSubdomainRoute(
                 "Blog_default", // Route name
                 "blog",
                 "{controller}/{action}/{username}/{page}",    // URL with parameters 
                 new { area = this.AreaName, controller = "Blog", action = "Index", username = UrlParameter.Optional, page = UrlParameter.Optional }  // Parameter defaults 
             );
            //context.Routes.MapSubDomainRoute(
            //      "Blog_default", // Route name
            //      "blog", // Domain with parameters 
            //      "{controller}/{action}",    // URL with parameters 
            //      new { controller = "Blog", action = "Index" },  // Parameter defaults 
            //      new[] { typeof(Controllers.BlogController).Namespace }
            //  );
            //context.MapRoute(
            //    "Blog_default",
            //    "{subdomain}/{controller}/{action}/{username}/{page}",
            //    new { subdomain = "blog", controller = "Blog", action = "Index", username = UrlParameter.Optional, page = UrlParameter.Optional },
            //    namespaces: new[] { "Teknik.Areas.Blog.Controllers" }
            //);
        }
    }
}