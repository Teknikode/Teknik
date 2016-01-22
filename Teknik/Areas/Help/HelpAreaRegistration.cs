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
                 "Help.API", // Route name
                 "dev",
                 "Help/API/{version}/{service}",    // URL with parameters 
                 new { controller = "Help", action = "API", version = UrlParameter.Optional, service = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Blog", // Route name
                 "dev",
                 "Help/Blog",    // URL with parameters 
                 new { controller = "Help", action = "Blog" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Git", // Route name
                 "dev",
                 "Help/Git",    // URL with parameters 
                 new { controller = "Help", action = "Git" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.IRC", // Route name
                 "dev",
                 "Help/IRC",    // URL with parameters 
                 new { controller = "Help", action = "IRC" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Mail", // Route name
                 "dev",
                 "Help/Mail",    // URL with parameters 
                 new { controller = "Help", action = "Mail" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Mumble", // Route name
                 "dev",
                 "Help/Mumble",    // URL with parameters 
                 new { controller = "Help", action = "Mumble" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Upload", // Route name
                 "dev",
                 "Help/Upload",    // URL with parameters 
                 new { controller = "Help", action = "Upload" },  // Parameter defaults 
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
                 "Help.API", // Route name
                 "help",
                 "API/{version}/{service}",    // URL with parameters 
                 new { controller = "Help", action = "API", version = UrlParameter.Optional, service = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Blog", // Route name
                 "help",
                 "Blog",    // URL with parameters 
                 new { controller = "Help", action = "Blog" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Git", // Route name
                 "help",
                 "Git",    // URL with parameters 
                 new { controller = "Help", action = "Git" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.IRC", // Route name
                 "help",
                 "IRC",    // URL with parameters 
                 new { controller = "Help", action = "IRC" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Mail", // Route name
                 "help",
                 "Mail",    // URL with parameters 
                 new { controller = "Help", action = "Mail" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Mumble", // Route name
                 "help",
                 "Mumble",    // URL with parameters 
                 new { controller = "Help", action = "Mumble" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Help.Upload", // Route name
                 "help",
                 "Upload",    // URL with parameters 
                 new { controller = "Help", action = "Upload" },  // Parameter defaults 
                 new[] { typeof(Controllers.HelpController).Namespace }
             );

            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/help").Include(
                      "~/Areas/Help/Content/Help.css"));
        }
    }
}