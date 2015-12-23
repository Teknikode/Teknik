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
                 "Blog.Blog", // Route name
                 "dev",
                 "Blog/{username}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Post", // Route name
                 "dev",
                 "Blog/{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Post", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Action", // Route name
                 "dev",
                 "Blog/Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Blog", // Route name
                 "blog",
                 "{username}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Post", // Route name
                 "blog",
                 "{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Post", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Action", // Route name
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
                      "~/Areas/Blog/Content/Blog.css",
                      "~/Content/bootstrap-markdown.min.css"));
        }
    }
}