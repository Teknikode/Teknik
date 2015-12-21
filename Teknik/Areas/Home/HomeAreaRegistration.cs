using System.Web.Mvc;
using System.Web.Optimization;
using Teknik;

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
            context.MapSubdomainRoute(
                 "Home_dev", // Route name
                 "dev",
                 "Home",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Home_subdomain", // Route name
                 "www",
                 "",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Home_default", // Route name
                 string.Empty,
                 "",    // URL with parameters 
                 new { controller = "Home", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.HomeController).Namespace }
             );

            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/home").Include(
                      "~/Areas/Home/Content/Home.css"));
        }
    }
}