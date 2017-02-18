using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Status
{
    public class StatusAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Status";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Status.Index",
                 new List<string>() { "status" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Status", action = "Index" },
                 new[] { typeof(Controllers.StatusController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Status.Action",
                 new List<string>() { "status" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",
                 new { controller = "Status", action = "Index" },
                 new[] { typeof(Controllers.StatusController).Namespace }
             );

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/status", config.CdnHost).Include(
                      "~/Scripts/Highcharts/highcharts.js",
                      "~/Scripts/FileSize/filesize.min.js",
                      "~/Areas/Status/Scripts/Status.js"));
        }
    }
}