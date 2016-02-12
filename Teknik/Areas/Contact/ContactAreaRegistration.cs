using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;

namespace Teknik.Areas.Contact
{
    public class ContactAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Contact";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Contact.Index", // Route name
                 new List<string>() { "dev", "contact" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Contact.Action", // Route name
                 new List<string>() { "dev", "contact" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{action}",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );

            // Register Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/contact").Include(
                      "~/Areas/Contact/Scripts/Contact.js"));
        }
    }
}