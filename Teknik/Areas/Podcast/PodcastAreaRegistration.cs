using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;

namespace Teknik.Areas.Podcast
{
    public class PodcastAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Podcast";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Podcast.Index", // Route name
                 new List<string>() { "podcast" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Podcast", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PodcastController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Podcast.View", // Route name
                 new List<string>() { "podcast" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{episode}",    // URL with parameters 
                 new { controller = "Podcast", action = "View" },  // Parameter defaults 
                 new[] { typeof(Controllers.PodcastController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Podcast.Download", // Route name
                 new List<string>() { "podcast" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "File/{episode}/{fileName}",    // URL with parameters 
                 new { controller = "Podcast", action = "Download" },  // Parameter defaults 
                 new[] { typeof(Controllers.PodcastController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Podcast.Action", // Route name
                 new List<string>() { "podcast" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",    // URL with parameters 
                 new { controller = "Podcast", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PodcastController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/podcast").Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/jquery.blockUI.js",
                      "~/Areas/Podcast/Scripts/Podcast.js"));
            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/podcast").Include(
                      "~/Areas/Podcast/Content/Podcast.css"));
        }
    }
}