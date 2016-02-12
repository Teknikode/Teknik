using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

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
                 new List<string>() { "dev", "transparency" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",
                 new { controller = "Transparency", action = "Index" },
                 new[] { typeof(Controllers.TransparencyController).Namespace }
             );
        }
    }
}