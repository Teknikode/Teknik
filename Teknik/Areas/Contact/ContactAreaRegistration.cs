using System.Web.Mvc;

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
                 "Contact_dev", // Route name
                 "dev",
                 "Contact/{controller}/{action}",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Contact_default", // Route name
                 "contact",
                 "{controller}/{action}",    // URL with parameters 
                 new { controller = "Contact", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ContactController).Namespace }
             );
        }
    }
}