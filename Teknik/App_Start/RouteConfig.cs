using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Teknik
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            string configContents = File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/Config.json"));
            Config config = Config.Deserialize(configContents);

            routes.MapSubdomainRoute(
                 "SubdomainRoute", // Route name
                 "{customer}." + config.Host, // Domain with parameters 
                 "{action}/{id}",    // URL with parameters 
                 new { controller = "Home", action = "Index", id = "" }  // Parameter defaults 
             );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
