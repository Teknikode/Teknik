﻿using System.Web.Mvc;

namespace Teknik.Areas.About
{
    public class AboutAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "About";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "About.Index", // Route name
                 "dev",
                 "About",    // URL with parameters 
                 new { controller = "About", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.AboutController).Namespace }
             );
            context.MapSubdomainRoute(
                 "About.Index", // Route name
                 "about",
                 "",    // URL with parameters 
                 new { controller = "About", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.AboutController).Namespace }
             );
        }
    }
}