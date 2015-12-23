using System.Web.Mvc;
using System.Web.Optimization;

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
            context.MapSubdomainRoute(
                 "Contact.Index", // Route name
                 "dev",
                 "Contact/",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Contact.Action", // Route name
                 "dev",
                 "Contact/{action}",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Contact.Index", // Route name
                 "contact",
                 "",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Contact.Action", // Route name
                 "contact",
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