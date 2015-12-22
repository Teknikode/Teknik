using System.Web.Mvc;

namespace Teknik.Areas.Profile
{
    public class ProfileAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Profile";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapSubdomainRoute(
                 "Profile_dev_login", // Route name
                 "dev",
                 "Profile/Login",    // URL with parameters 
                 new { controller = "Profile", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile_dev_logout", // Route name
                 "dev",
                 "Profile/Logout",    // URL with parameters 
                 new { controller = "Profile", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile_dev_Register", // Route name
                 "dev",
                 "Profile/Login",    // URL with parameters 
                 new { controller = "Profile", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile_dev", // Route name
                 "dev",
                 "Profile/{username}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile_dev_unique", // Route name
                 "dev",
                 "Profile/{controller}/{action}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile_default_login", // Route name
                 "profile",
                 "Login",    // URL with parameters 
                 new { controller = "Profile", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile_default_logout", // Route name
                 "profile",
                 "Logout",    // URL with parameters 
                 new { controller = "Profile", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile_default_register", // Route name
                 "profile",
                 "Register",    // URL with parameters 
                 new { controller = "Profile", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile_default", // Route name
                 "profile",
                 "{username}",    // URL with parameters 
                 new { controller = "Profile", action = "Index", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile_default_unique", // Route name
                 "profile",
                 "{controller}/{action}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
        }
    }
}