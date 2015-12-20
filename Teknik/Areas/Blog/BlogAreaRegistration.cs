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
                 new { controller = "Blog", action = "Blog", username = string.Empty },  // Parameter defaults 
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
                 "Blog_dev_post_unique", // Route name
                 "dev",
                 "Blog/Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_blog", // Route name
                 "blog",
                 "{username}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_post", // Route name
                 "blog",
                 "{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Post", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_post_unique", // Route name
                 "blog",
                 "Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/blog").Include(
                      "~/Scripts/ocupload/1.1.2/ocupload.js",
                      "~/Scripts/bootstrap/markdown/bootstrap-markdown.js",
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Areas/Blog/Scripts/Blog.js"));
            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/blog").Include(
                      "~/Content/bootstrap-markdown.min.css"));
        }
    }
}