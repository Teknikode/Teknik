using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;

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
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Blog.Blog", // Route name
                 new List<string>() { "blog" }, // Subdomains
                 new List<string>() { config.Host },
                 "{username}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog", username = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.New", // Route name
                 new List<string>() { "blog" }, // Subdomains
                 new List<string>() { config.Host },
                 "{username}/New",    // URL with parameters 
                 new { controller = "Blog", action = "NewPost", username = "" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Edit", // Route name
                 new List<string>() { "blog" }, // Subdomains
                 new List<string>() { config.Host },
                 "{username}/Edit/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "EditPost", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Post", // Route name
                 new List<string>() { "blog" }, // Subdomains
                 new List<string>() { config.Host },
                 "{username}/p/{id}",    // URL with parameters 
                 new { controller = "Blog", action = "Post", username = "", id = 0 },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Blog.Action", // Route name
                 new List<string>() { "blog" }, // Subdomains
                 new List<string>() { config.Host },
                 "Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Blog", action = "Blog" },  // Parameter defaults 
                 new[] { typeof(Controllers.BlogController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/blog").Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/MarkdownDeepLib.min.js",
                      "~/Areas/Blog/Scripts/Blog.js"));
            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/blog").Include(
                      "~/Scripts/mdd_styles.css",
                      "~/Areas/Blog/Content/Blog.css"));
        }
    }
}