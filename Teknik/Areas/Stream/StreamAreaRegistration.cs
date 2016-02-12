﻿using System.Collections.Generic;
using System.Web.Mvc;
using Teknik.Configuration;

namespace Teknik.Areas.Stream
{
    public class StreamAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Stream";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Stream.Index", // Route name
                 new List<string>() { "dev", "stream" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Stream", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.StreamController).Namespace }
             );
        }
    }
}