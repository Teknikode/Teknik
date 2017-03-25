using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.IRC
{
    public class IRCAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "IRC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "IRC.Index", // Route name
                 new List<string>() { "irc" },
                 new List<string>() { config.Host },
                 "",    // URL with parameters 
                 new { controller = "IRC", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.IRCController).Namespace }
             );

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/irc", config.CdnHost).Include(
                      "~/Areas/IRC/Scripts/IRC.js"));
        }
    }
}