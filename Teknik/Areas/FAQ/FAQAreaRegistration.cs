using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.FAQ
{
    public class FAQAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FAQ";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "FAQ.Index", // Route name
                 new List<string>() { "faq" },
                 new List<string>() { config.Host },
                 "",    // URL with parameters 
                 new { controller = "FAQ", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.FAQController).Namespace }
             );

            // Register Style Bundle
            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/faq", config.CdnHost).Include(
                      "~/Areas/FAQ/Content/FAQ.css"));
        }
    }
}