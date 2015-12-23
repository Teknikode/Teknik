using System.Web.Mvc;

namespace Teknik.Areas.Error
{
    public class ErrorAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Error";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Error_404", // Route name
                 "*",
                 "Error/404",    // URL with parameters 
                 new { controller = "Error", action = "Http404" },  // Parameter defaults 
                 new[] { typeof(Controllers.ErrorController).Namespace }
             );
        }
    }
}