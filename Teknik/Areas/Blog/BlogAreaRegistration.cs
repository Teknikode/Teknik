using System.Web.Mvc;
using System.Web.Optimization;

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
                 "Blog/{controller}/{action}/{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Index", username = UrlParameter.Optional, id = UrlParameter.Optional }  // Parameter defaults 
             );
            context.MapSubdomainRoute(
                 "Blog_default", // Route name
                 "blog",
                 "{controller}/{action}/{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Index", username = UrlParameter.Optional, id = UrlParameter.Optional }  // Parameter defaults 
             );

            // Register Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/blog").Include(
                      "~/Areas/Blog/Scripts/Blog.js"));
        }
    }
}