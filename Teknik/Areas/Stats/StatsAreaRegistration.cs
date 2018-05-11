using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Stats
{
    public class StatsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Stats";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Stats.Index",
                 new List<string>() { "stats" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Stats", action = "Index" },
                 new[] { typeof(Controllers.StatsController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Stats.Action",
                 new List<string>() { "stats" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",
                 new { controller = "Stats", action = "Index" },
                 new[] { typeof(Controllers.StatsController).Namespace }
             );

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/stats", config.CdnHost).Include(
                      "~/Scripts/Highcharts/highcharts.js",
                      "~/Scripts/FileSize/filesize.min.js",
                      "~/Areas/Stats/Scripts/Stats.js"));
        }
    }
}
