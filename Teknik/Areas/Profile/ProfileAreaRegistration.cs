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
                 "Profile.Login", // Route name
                 "dev",
                 "Profile/Login",    // URL with parameters 
                 new { controller = "Profile", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile.Logout", // Route name
                 "dev",
                 "Profile/Logout",    // URL with parameters 
                 new { controller = "Profile", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile.Register", // Route name
                 "dev",
                 "Profile/Register",    // URL with parameters 
                 new { controller = "Profile", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile.Index", // Route name
                 "dev",
                 "Profile/{username}",    // URL with parameters 
                 new { controller = "Profile", action = "Index", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile.Action", // Route name
                 "dev",
                 "Profile/{controller}/{action}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Profile.Login", // Route name
                 "profile",
                 "Login",    // URL with parameters 
                 new { controller = "Profile", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Logout", // Route name
                 "profile",
                 "Logout",    // URL with parameters 
                 new { controller = "Profile", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Register", // Route name
                 "profile",
                 "Register",    // URL with parameters 
                 new { controller = "Profile", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Index", // Route name
                 "profile",
                 "{username}",    // URL with parameters 
                 new { controller = "Profile", action = "Index", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
            context.MapSubdomainRoute(
                 "Profile.Action", // Route name
                 "profile",
                 "{controller}/{action}",    // URL with parameters 
                 new { controller = "Profile", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.ProfileController).Namespace }
            );
        }
    }
}