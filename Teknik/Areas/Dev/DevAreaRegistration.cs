using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Areas.Home.Controllers;

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
            context.MapSubdomainRoute(
                 "Dev.Index", // Route name
                 new List<string>() { "dev" }, // Subdomains
                 "",    // URL with parameters 
                 new { controller = "Dev", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.DevController).Namespace }
             );
        }
    }
}