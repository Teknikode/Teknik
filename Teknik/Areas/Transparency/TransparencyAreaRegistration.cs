using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Transparency
{
    public class TransparencyAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Transparency";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Transparency.Index",
                 new List<string>() { "transparency" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Transparency", action = "Index" },
                 new[] { typeof(Controllers.TransparencyController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Transparency.Action",
                 new List<string>() { "transparency" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",
                 new { controller = "Transparency", action = "Index" },
                 new[] { typeof(Controllers.TransparencyController).Namespace }
             );

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/transparency", config.CdnHost).Include(
                      "~/Scripts/Highcharts/highcharts.js",
                      "~/Areas/Transparency/Scripts/Transparency.js"));
        }
    }
}