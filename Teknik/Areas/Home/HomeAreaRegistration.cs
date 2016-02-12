﻿using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik;
using Teknik.Configuration;

namespace Teknik.Areas.Home
{
    public class HomeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Home";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Home.Index", // Route name
                 new List<string>() { "dev", "www", string.Empty }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );

            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/home").Include(
                      "~/Areas/Home/Content/Home.css"));

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/home").Include(
                      "~/Scripts/PageDown/Markdown.Converter.js",
                      "~/Scripts/PageDown/Markdown.Sanitizer.js"));
        }
    }
}