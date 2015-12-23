using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Teknik.Areas.Error.Controllers;

namespace Teknik
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapMvcAttributeRoutes();
            //routes.Add(new SubdomainRoute());

            //Map subdomains to specific style of routing
            // Development Routing
            //routes.MapSubdomainRoute(
            //     "Dev", // Route name
            //     "dev." + config.Host, // Domain with parameters 
            //     "{controller}/{action}",    // URL with parameters 
            //     new { controller = "Dev", action = "Index" }  // Parameter defaults 
            // );
            //// Blog Routing
            //routes.MapSubdomainRoute(
            //     "Blog", // Route name
            //     "{controller}." + config.Host, // Domain with parameters 
            //     "{username}/{id}",    // URL with parameters 
            //     new { controller = "Blog", action = "Index" }  // Parameter defaults 
            // );

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}",
            //    defaults: new { controller = "Home", action = "Index" },
            //    namespaces: new[] { "Teknik.Controllers" }
            //);
        }
    }
}
