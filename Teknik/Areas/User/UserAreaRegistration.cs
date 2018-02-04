using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Areas.Users
{
    public class UserAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "User.GetPremium", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "GetPremium",    // URL with parameters 
                 new { controller = "User", action = "GetPremium" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Login", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Login",    // URL with parameters 
                 new { controller = "User", action = "Login" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Logout", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Logout",    // URL with parameters 
                 new { controller = "User", action = "Logout" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Register", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Register",    // URL with parameters 
                 new { controller = "User", action = "Register" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Settings", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Settings",    // URL with parameters 
                 new { controller = "User", action = "Settings" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                "User.SecuritySettings", // Route name
                new List<string>() { "user" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "Settings/Security",    // URL with parameters 
                new { controller = "User", action = "SecuritySettings" },  // Parameter defaults 
                new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                "User.ProfileSettings", // Route name
                new List<string>() { "user" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "Settings/Profile",    // URL with parameters 
                new { controller = "User", action = "ProfileSettings" },  // Parameter defaults 
                new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                "User.InviteSettings", // Route name
                new List<string>() { "user" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "Settings/Invites",    // URL with parameters 
                new { controller = "User", action = "InviteSettings" },  // Parameter defaults 
                new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                "User.BlogSettings", // Route name
                new List<string>() { "user" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "Settings/Blog",    // URL with parameters 
                new { controller = "User", action = "BlogSettings" },  // Parameter defaults 
                new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                "User.UploadSettings", // Route name
                new List<string>() { "user" }, // Subdomains
                new List<string>() { config.Host }, // domains
                "Settings/Uploads",    // URL with parameters 
                new { controller = "User", action = "UploadSettings" },  // Parameter defaults 
                new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.ResetPassword", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Reset/{username}",    // URL with parameters 
                 new { controller = "User", action = "ResetPassword", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.VerifyResetPassword", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Reset/{username}/{code}",    // URL with parameters 
                 new { controller = "User", action = "VerifyResetPassword" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.VerifyRecoveryEmail", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "VerifyEmail/{code}",    // URL with parameters 
                 new { controller = "User", action = "VerifyRecoveryEmail" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.CheckAuthenticatorCode", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "CheckAuthCode",    // URL with parameters 
                 new { controller = "User", action = "ConfirmTwoFactorAuth" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.ViewProfile", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "u/{username}",    // URL with parameters 
                 new { controller = "User", action = "ViewProfile", username = UrlParameter.Optional },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.PGPKey", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "u/{username}/PGP",    // URL with parameters 
                 new { controller = "User", action = "ViewRawPGP" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );
            context.MapSubdomainRoute(
                 "User.Action", // Route name
                 new List<string>() { "user" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{action}",    // URL with parameters 
                 new { controller = "User", action = "Index" },  // Parameter defaults 
                 new[] { typeof(Controllers.UserController).Namespace }
            );

            // Register Script Bundle
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user", config.CdnHost).Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/jquery.blockUI.js",
                      "~/Scripts/bootstrap-switch.js",
                      "~/Areas/User/Scripts/User.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/reset", config.CdnHost).Include(
                "~/Areas/User/Scripts/ResetPass.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/settings", config.CdnHost).Include(
                "~/Scripts/bootbox/bootbox.min.js",
                "~/Areas/User/Scripts/Settings.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/settings/blog", config.CdnHost).Include(
                "~/Areas/User/Scripts/BlogSettings.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/settings/invite", config.CdnHost).Include(
                "~/Areas/User/Scripts/InviteSettings.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/settings/profile", config.CdnHost).Include(
                "~/Areas/User/Scripts/ProfileSettings.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/settings/security", config.CdnHost).Include(
                "~/Scripts/bootstrap-switch.js",
                "~/Areas/User/Scripts/SecuritySettings.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/user/settings/upload", config.CdnHost).Include(
                "~/Scripts/bootstrap-switch.js",
                "~/Areas/User/Scripts/UploadSettings.js"));

            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/checkAuthCode", config.CdnHost).Include(
                      "~/Areas/User/Scripts/CheckAuthCode.js"));
            
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/profile", config.CdnHost).Include(
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Areas/User/Scripts/Profile.js"));

            // Register Style Bundles
            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/user", config.CdnHost).Include(
                      "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css"));
            
            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/user/settings/security", config.CdnHost).Include(
                "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css"));

            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/user/settings/upload", config.CdnHost).Include(
                "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css"));
        }
    }
}
