using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;

namespace Teknik.Areas.Paste
{
    public class PasteAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Paste";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Paste.Index", // Route name
                 new List<string>() { "dev", "paste", "p" },
                 "",    // URL with parameters 
                 new { controller = "Paste", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.View", // Route name
                 new List<string>() { "dev", "paste", "p" },
                 "{id}",    // URL with parameters 
                 new { controller = "Paste", action = "ViewPaste" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Simple", // Route name
                 new List<string>() { "dev", "paste", "p" },
                 "Simple/{id}",    // URL with parameters 
                 new { controller = "Paste", action = "Simple" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Raw", // Route name
                 new List<string>() { "dev", "paste", "p" },
                 "Raw/{id}",    // URL with parameters 
                 new { controller = "Paste", action = "Raw" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Action", // Route name
                 new List<string>() { "dev", "paste", "p" },
                 "Action/{action}",    // URL with parameters 
                 new { controller = "Paste", action = string.Empty },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/paste").Include(
                      "~/Scripts/Highlight/highlight.pack.js",
                      "~/Areas/Paste/Scripts/Paste.js"));
            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/paste").Include(
                      "~/Content/Highlight/default.css"));
        }
    }
}