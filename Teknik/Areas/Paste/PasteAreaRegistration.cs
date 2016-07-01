using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Controllers;

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
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Paste.Index", // Route name
                 new List<string>() { "paste", "p" },
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Paste", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Favicon",
                 new List<string>() { "paste", "p" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "favicon.ico",
                 new { controller = "Default", action = "Favicon" },
                 new[] { typeof(DefaultController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Simple", // Route name
                 new List<string>() { "paste", "p" },
                 new List<string>() { config.Host }, // domains
                 "Simple/{url}",    // URL with parameters 
                 new { controller = "Paste", action = "ViewPaste", type = "Simple" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Raw", // Route name
                 new List<string>() { "paste", "p" },
                 new List<string>() { config.Host }, // domains
                 "Raw/{url}",    // URL with parameters 
                 new { controller = "Paste", action = "ViewPaste", type = "Raw" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Download", // Route name
                 new List<string>() { "paste", "p" },
                 new List<string>() { config.Host }, // domains
                 "Download/{url}",    // URL with parameters 
                 new { controller = "Paste", action = "ViewPaste", type = "Download" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.Action", // Route name
                 new List<string>() { "paste", "p" },
                 new List<string>() { config.Host }, // domains
                 "Action/{action}",    // URL with parameters 
                 new { controller = "Paste", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Paste.View", // Route name
                 new List<string>() { "paste", "p" },
                 new List<string>() { config.Host }, // domains
                 "{url}",    // URL with parameters 
                 new { controller = "Paste", action = "ViewPaste", type = "Full" },  // Parameter defaults 
                 new[] { typeof(Controllers.PasteController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/paste").Include(
                      "~/Scripts/Highlight/highlight.pack.js",
                      "~/Areas/Paste/Scripts/Paste.js"));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/syntaxWorker").Include(
                      "~/Areas/Paste/Scripts/SyntaxWorker.js"));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/highlight").Include(
                      "~/Scripts/Highlight/highlight.pack.js"));
            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/paste").Include(
                      "~/Content/Highlight/github-gist.css",
                      "~/Areas/Paste/Content/Paste.css"));
        }
    }
}