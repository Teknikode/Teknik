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
                 "Blog_dev_blog", // Route name
                 "dev",
                 "Blog/{username}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_dev_post", // Route name
                 "dev",
                 "Blog/{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Post", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_blog", // Route name
                 "blog",
                 "{username}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_post", // Route name
                 "blog",
                 "{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Post", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );

            // Register Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/blog").Include(
                      "~/Scripts/ocupload/1.1.2/ocupload.js",
                      "~/Areas/Blog/Scripts/Blog.js"));
        }
    }
}