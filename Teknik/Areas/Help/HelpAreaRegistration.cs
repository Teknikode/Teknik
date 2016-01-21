using System.Web.Mvc;
using System.Web.Optimization;

namespace Teknik.Areas.Help
{
    public class HelpAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Help";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Help.Index", // Route name
                 "dev",
                 "Help",    // URL with parameters 
                 new { controller = "Help", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Topic", // Route name
                 "dev",
                 "Help/{topic}",    // URL with parameters 
                 new { controller = "Help", action = "Topic" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Index", // Route name
                 "help",
                 "",    // URL with parameters 
                 new { controller = "Help", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Topic", // Route name
                 "help",
                 "{topic}",    // URL with parameters 
                 new { controller = "Help", action = "Topic" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );

            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/help").Include(
                      "~/Areas/Help/Content/Help.css"));
        }
    }
}