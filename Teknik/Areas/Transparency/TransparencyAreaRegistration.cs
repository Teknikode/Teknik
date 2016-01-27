using System.Collections.Generic;
using System.Web.Mvc;

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
            context.MapSubdomainRoute(
                 "Transparency.Index",
                 new List<string>() { "dev", "transparency" }, // Subdomains
                 "",
                 new { controller = "Transparency", action = "Index" },
                 new[] { typeof(Controllers.TransparencyController).Namespace }
             );
        }
    }
}