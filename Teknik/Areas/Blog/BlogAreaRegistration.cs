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
                 "Blog_dev_index", // Route name
                 "dev",
                 "Blog",    // URL with parameters 
                 new { controller = "Blog", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_dev_detail", // Route name
                 "dev",
                 "Blog/{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Details", username = "", id = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_index", // Route name
                 "blog",
                 "",    // URL with parameters 
                 new { controller = "Blog", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog_default_detail", // Route name
                 "blog",
                 "{username}/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Details", username = "", id = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );

            // Register Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/blog").Include(
                      "~/Scripts/ocupload/1.1.2/ocupload.js",
                      "~/Areas/Blog/Scripts/Blog.js"));
        }
    }
}