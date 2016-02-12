using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Areas.Home.Controllers;
using Teknik.Configuration;

namespace Teknik.Areas.Dev
{
    public class DevAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Dev";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Dev.Index", // Route name
                 new List<string>() { "dev" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Dev", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.DevController).Namespace }
             );
        }
    }
}