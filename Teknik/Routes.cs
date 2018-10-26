using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik
{
    public static class Routes
    {
        public static void BuildRoutes(this IRouteBuilder routes, Config config)
        {
            routes.BuildDefaultRoutes(config);
            routes.BuildAboutRoutes(config);
            routes.BuildAbuseRoutes(config);
            routes.BuildAdminRoutes(config);
            routes.BuildAPIRoutes(config);
            routes.BuildBlogRoutes(config);
            routes.BuildContactRoutes(config);
            routes.BuildDevRoutes(config);
            routes.BuildErrorRoutes(config);
            routes.BuildFAQRoutes(config);
            routes.BuildHelpRoutes(config);
            routes.BuildHomeRoutes(config);
            routes.BuildPasteRoutes(config);
            routes.BuildPodcastRoutes(config);
            routes.BuildPrivacyRoutes(config);
            routes.BuildRSSRoutes(config);
            routes.BuildShortenerRoutes(config);
            routes.BuildStatsRoutes(config);
            routes.BuildTOSRoutes(config);
            routes.BuildUploadRoutes(config);
            routes.BuildUserRoutes(config);
            routes.BuildVaultRoutes(config);
        }

        public static void BuildDefaultRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Default.Favicon",
              domains: new List<string>() { config.Host, config.ShortenerConfig.ShortenerHost },
              subDomains: new List<string>() { "*" },
              template: "favicon.ico",
              defaults: new { area = "Default", controller = "Default", action = "Favicon" }
            );
            routes.MapSubdomainRoute(
              name: "Default.Logo",
              domains: new List<string>() { config.Host, config.ShortenerConfig.ShortenerHost },
              subDomains: new List<string>() { "*" },
              template: "logo.svg",
              defaults: new { area = "Default", controller = "Default", action = "Logo" }
            );
            routes.MapSubdomainRoute(
              name: "Default.Robots",
              domains: new List<string>() { config.Host, config.ShortenerConfig.ShortenerHost },
              subDomains: new List<string>() { "*" },
              template: "robots.txt",
              defaults: new { area = "Default", controller = "Default", action = "Robots" }
            );
            routes.MapSubdomainRoute(
              name: "Default.NotFound",
              domains: new List<string>() { config.Host, config.ShortenerConfig.ShortenerHost },
              subDomains: new List<string>() { "*" },
              template: "{url}",
              defaults: new { area = "Error", controller = "Error", action = "Http404" },
              constraints: new { url = "{*url}" }
            );
        }

        public static void BuildAboutRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "About.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "about" },
              template: "",
              defaults: new { area = "About", controller = "About", action = "Index" }
            );
        }

        public static void BuildAbuseRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Abuse.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "abuse" },
              template: "",
              defaults: new { area = "Abuse", controller = "Abuse", action = "Index" }
            );
        }

        public static void BuildAdminRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Admin.Dashboard",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "admin" },
              template: "",
              defaults: new { area = "Admin", controller = "Admin", action = "Dashboard" }
            );
            routes.MapSubdomainRoute(
              name: "Admin.UserSearch",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "admin" },
              template: "Search/Users",
              defaults: new { area = "Admin", controller = "Admin", action = "UserSearch" }
            );
            routes.MapSubdomainRoute(
              name: "Admin.UploadSearch",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "admin" },
              template: "Search/Uploads",
              defaults: new { area = "Admin", controller = "Admin", action = "UploadSearch" }
            );
            routes.MapSubdomainRoute(
              name: "Admin.UserInfo",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "admin" },
              template: "User/{username}",
              defaults: new { area = "Admin", controller = "Admin", action = "UserInfo", username = string.Empty }
            );
            routes.MapSubdomainRoute(
              name: "Admin.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "admin" },
              template: "Action/{action}",
              defaults: new { area = "Admin", controller = "Admin", action = "Dashboard" }
            );
        }

        public static void BuildAPIRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "API.v1.Claims",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "api" },
              template: "v1/Claims",
              defaults: new { area = "API", controller = "AccountAPIv1", action = "GetClaims" }
            );
            routes.MapSubdomainRoute(
              name: "API.v1.Upload",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "api" },
              template: "v1/Upload",
              defaults: new { area = "API", controller = "UploadAPIv1", action = "Upload" }
            );
            routes.MapSubdomainRoute(
              name: "API.v1.Paste",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "api" },
              template: "v1/Paste",
              defaults: new { area = "API", controller = "PasteAPIv1", action = "Paste" }
            );
            routes.MapSubdomainRoute(
              name: "API.v1.Shorten",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "api" },
              template: "v1/Shorten",
              defaults: new { area = "API", controller = "ShortenAPIv1", action = "Shorten" }
            );

            routes.MapSubdomainRoute(
              name: "API.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "api" },
              template: "",
              defaults: new { area = "API", controller = "API", action = "Index" }
            );
        }

        public static void BuildBlogRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Blog.Blog",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "blog" },
              template: "{username}",
              defaults: new { area = "Blog", controller = "Blog", action = "Blog", username = string.Empty }
            );
            routes.MapSubdomainRoute(
              name: "Blog.New",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "blog" },
              template: "{username}/New",
              defaults: new { area = "Blog", controller = "Blog", action = "NewPost", username = string.Empty }
            );
            routes.MapSubdomainRoute(
              name: "Blog.Edit",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "blog" },
              template: "{username}/Edit/{id}",
              defaults: new { area = "Blog", controller = "Blog", action = "EditPost", username = string.Empty, id = -1 }
            );
            routes.MapSubdomainRoute(
              name: "Blog.Post",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "blog" },
              template: "{username}/p/{id}",
              defaults: new { area = "Blog", controller = "Blog", action = "Post", username = string.Empty, id = -1 }
            );
            routes.MapSubdomainRoute(
              name: "Blog.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "blog" },
              template: "Action/{action}",
              defaults: new { area = "Blog", controller = "Blog", action = "Blog" }
            );
        }

        public static void BuildContactRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Contact.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "contact" },
              template: "",
              defaults: new { area = "Contact", controller = "Contact", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Contact.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "contact" },
              template: "Action/{action}",
              defaults: new { area = "Contact", controller = "Contact", action = "Index" }
            );
        }

        public static void BuildDevRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Dev.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "dev" },
              template: "",
              defaults: new { area = "Dev", controller = "Dev", action = "Index" }
            );
        }

        public static void BuildErrorRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Error.HttpError",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "error" },
              template: "{statusCode:int}",
              defaults: new { area = "Error", controller = "Error", action = "HttpError", statusCode = 500 }
            );
            routes.MapSubdomainRoute(
              name: "Error.Http401",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "error" },
              template: "401",
              defaults: new { area = "Error", controller = "Error", action = "Http401" }
            );
            routes.MapSubdomainRoute(
              name: "Error.Http403",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "error" },
              template: "403",
              defaults: new { area = "Error", controller = "Error", action = "Http403" }
            );
            routes.MapSubdomainRoute(
              name: "Error.Http404",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "error" },
              template: "404",
              defaults: new { area = "Error", controller = "Error", action = "Http404" }
            );
            routes.MapSubdomainRoute(
              name: "Error.Http500",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "error" },
              template: "500",
              defaults: new { area = "Error", controller = "Error", action = "Http500" }
            );
            routes.MapSubdomainRoute(
              name: "Error.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "error" },
              template: "Action/{action}",
              defaults: new { area = "Error", controller = "Error", action = "HttpError" }
            );
        }

        public static void BuildFAQRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "FAQ.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "faq" },
              template: "",
              defaults: new { area = "FAQ", controller = "FAQ", action = "Index" }
            );
        }

        public static void BuildHelpRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Help.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "",
              defaults: new { area = "Help", controller = "Help", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Help.API",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "API/{version?}/{service?}",
              defaults: new { area = "Help", controller = "Help", action = "API" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Blog",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Blog",
              defaults: new { area = "Help", controller = "Help", action = "Blog" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Git",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Git",
              defaults: new { area = "Help", controller = "Help", action = "Git" }
            );
            routes.MapSubdomainRoute(
              name: "Help.IRC",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "IRC",
              defaults: new { area = "Help", controller = "Help", action = "IRC" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Mail",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Mail",
              defaults: new { area = "Help", controller = "Help", action = "Mail" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Markdown",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Markdown",
              defaults: new { area = "Help", controller = "Help", action = "Markdown" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Mumble",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Mumble",
              defaults: new { area = "Help", controller = "Help", action = "Mumble" }
            );
            routes.MapSubdomainRoute(
              name: "Help.RSS",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "RSS",
              defaults: new { area = "Help", controller = "Help", action = "RSS" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Tools",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Tools",
              defaults: new { area = "Help", controller = "Help", action = "Tools" }
            );
            routes.MapSubdomainRoute(
              name: "Help.Upload",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "help" },
              template: "Upload",
              defaults: new { area = "Help", controller = "Help", action = "Upload" }
            );
        }

        public static void BuildHomeRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Home.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "www", string.Empty },
              template: "",
              defaults: new { area = "Home", controller = "Home", action = "Index" }
            );
        }

        public static void BuildPasteRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Paste.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "paste", "p" },
              template: "",
              defaults: new { area = "Paste", controller = "Paste", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Paste.Simple",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "paste", "p" },
              template: "Simple/{url}/{password?}",
              defaults: new { area = "Paste", controller = "Paste", action = "ViewPaste", type = "Simple" }
            );
            routes.MapSubdomainRoute(
              name: "Paste.Raw",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "paste", "p" },
              template: "Raw/{url}/{password?}",
              defaults: new { area = "Paste", controller = "Paste", action = "ViewPaste", type = "Raw" }
            );
            routes.MapSubdomainRoute(
              name: "Paste.Download",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "paste", "p" },
              template: "Download/{url}/{password?}",
              defaults: new { area = "Paste", controller = "Paste", action = "ViewPaste", type = "Download" }
            );
            routes.MapSubdomainRoute(
              name: "Paste.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "paste", "p" },
              template: "Action/{action}",
              defaults: new { area = "Paste", controller = "Paste", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Paste.View",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "paste", "p" },
              template: "{url}/{password?}",
              defaults: new { area = "Paste", controller = "Paste", action = "ViewPaste", type = "Full" }
            );
        }

        public static void BuildPodcastRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Podcast.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "podcast" },
              template: "",
              defaults: new { area = "Podcast", controller = "Podcast", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Podcast.View",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "podcast" },
              template: "ep/{episode}",
              defaults: new { area = "Podcast", controller = "Podcast", action = "View" }
            );
            routes.MapSubdomainRoute(
              name: "Podcast.Download",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "podcast" },
              template: "File/{episode}/{fileName}",
              defaults: new { area = "Podcast", controller = "Podcast", action = "Download" }
            );
            routes.MapSubdomainRoute(
              name: "Podcast.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "podcast" },
              template: "Action/{action}",
              defaults: new { area = "Podcast", controller = "Podcast", action = "Index" }
            );
        }

        public static void BuildPrivacyRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Privacy.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "privacy" },
              template: "",
              defaults: new { area = "Privacy", controller = "Privacy", action = "Index" }
            );
        }

        public static void BuildRSSRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "RSS.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "rss" },
              template: "",
              defaults: new { area = "RSS", controller = "RSS", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "RSS.Blog",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "rss" },
              template: "Blog/{username?}",
              defaults: new { area = "RSS", controller = "RSS", action = "Blog" }
            );
            routes.MapSubdomainRoute(
              name: "RSS.Podcast",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "rss" },
              template: "Podcast",
              defaults: new { area = "RSS", controller = "RSS", action = "Podcast" }
            );
        }

        public static void BuildShortenerRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Shortener.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "shorten", "s" },
              template: "",
              defaults: new { area = "Shortener", controller = "Shortener", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Shortener.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "shorten", "s" },
              template: "Action/{action}",
              defaults: new { area = "Shortener", controller = "Shortener", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Shortener.View",
              domains: new List<string>() { config.ShortenerConfig.ShortenerHost },
              subDomains: new List<string>() { string.Empty, "shortened" },
              template: "{url}",
              defaults: new { area = "Shortener", controller = "Shortener", action = "RedirectToUrl" }
            );
            routes.MapSubdomainRoute(
              name: "Shortener.Verify",
              domains: new List<string>() { config.ShortenerConfig.ShortenerHost },
              subDomains: new List<string>() { string.Empty },
              template: "",
              defaults: new { area = "Shortener", controller = "Shortener", action = "Verify" }
            );
        }

        public static void BuildStatsRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Stats.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "stats" },
              template: "",
              defaults: new { area = "Stats", controller = "Stats", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Stats.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "stats" },
              template: "Action/{action}",
              defaults: new { area = "Stats", controller = "Stats", action = "Index" }
            );
        }

        public static void BuildTOSRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "TOS.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "tos" },
              template: "",
              defaults: new { area = "TOS", controller = "TOS", action = "Index" }
            );
        }

        public static void BuildUploadRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Upload.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "upload", "u" },
              template: "",
              defaults: new { area = "Upload", controller = "Upload", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Upload.GenerateDeleteKey",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "upload", "u", "user" },
              template: "GenerateDeleteKey",
              defaults: new { area = "Upload", controller = "Upload", action = "GenerateDeleteKey" }
            );
            routes.MapSubdomainRoute(
              name: "Upload.Download",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "upload", "u" },
              template: "{file}",
              defaults: new { area = "Upload", controller = "Upload", action = "Download" }
            );
            routes.MapSubdomainRoute(
              name: "Upload.Delete",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "upload", "u" },
              template: "{file}/{key}",
              defaults: new { area = "Upload", controller = "Upload", action = "Delete" }
            );
            routes.MapSubdomainRoute(
              name: "Upload.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "upload", "u" },
              template: "Action/{action}",
              defaults: new { area = "Upload", controller = "Upload", action = "Index" }
            );
        }

        public static void BuildUserRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "User.GetPremium",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "GetPremium",
              defaults: new { area = "User", controller = "User", action = "GetPremium" }
            );
            routes.MapSubdomainRoute(
              name: "User.Register",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Register",
              defaults: new { area = "User", controller = "User", action = "Register" }
            );
            routes.MapSubdomainRoute(
              name: "User.Login",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Login",
              defaults: new { area = "User", controller = "User", action = "Login" }
            );
            routes.MapSubdomainRoute(
              name: "User.Logout",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Logout",
              defaults: new { area = "User", controller = "User", action = "Logout" }
            );
            routes.MapSubdomainRoute(
              name: "User.Settings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings",
              defaults: new { area = "User", controller = "User", action = "Settings" }
            );
            routes.MapSubdomainRoute(
              name: "User.AccountSettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/Account",
              defaults: new { area = "User", controller = "User", action = "AccountSettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.SecuritySettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/Security",
              defaults: new { area = "User", controller = "User", action = "SecuritySettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.ProfileSettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/Profile",
              defaults: new { area = "User", controller = "User", action = "ProfileSettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.AccessTokenSettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/AccessTokens",
              defaults: new { area = "User", controller = "User", action = "AccessTokenSettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.InviteSettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/Invites",
              defaults: new { area = "User", controller = "User", action = "InviteSettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.BlogSettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/Blog",
              defaults: new { area = "User", controller = "User", action = "BlogSettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.UploadSettings",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Settings/Uploads",
              defaults: new { area = "User", controller = "User", action = "UploadSettings" }
            );
            routes.MapSubdomainRoute(
              name: "User.ResetPassword",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "ResetPassword/{username?}",
              defaults: new { area = "User", controller = "User", action = "ResetPassword" }
            );
            routes.MapSubdomainRoute(
              name: "User.VerifyResetPassword",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "SetPassword/{username}",
              defaults: new { area = "User", controller = "User", action = "VerifyResetPassword" }
            );
            routes.MapSubdomainRoute(
              name: "User.VerifyRecoveryEmail",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "VerifyEmail/{username}",
              defaults: new { area = "User", controller = "User", action = "VerifyRecoveryEmail" }
            );
            routes.MapSubdomainRoute(
              name: "User.ViewProfile",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "u/{username?}",
              defaults: new { area = "User", controller = "User", action = "ViewProfile" }
            );
            routes.MapSubdomainRoute(
              name: "User.PGPKey",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "u/{username}/PGP",
              defaults: new { area = "User", controller = "User", action = "ViewRawPGP" }
            );
            routes.MapSubdomainRoute(
              name: "User.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "user" },
              template: "Action/{action}",
              defaults: new { area = "User", controller = "User", action = "Index" }
            );
        }

        public static void BuildVaultRoutes(this IRouteBuilder routes, Config config)
        {
            routes.MapSubdomainRoute(
              name: "Vault.Index",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "",
              defaults: new { area = "Vault", controller = "Vault", action = "Index" }
            );
            routes.MapSubdomainRoute(
              name: "Vault.NewVault",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "Create",
              defaults: new { area = "Vault", controller = "Vault", action = "NewVault" }
            );
            routes.MapSubdomainRoute(
              name: "Vault.NewVaultFromService",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "Create/{type}",
              defaults: new { area = "Vault", controller = "Vault", action = "NewVaultFromService" }
            );
            routes.MapSubdomainRoute(
              name: "Vault.EditVault",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "Edit/{url}",
              defaults: new { area = "Vault", controller = "Vault", action = "EditVault" }
            );
            routes.MapSubdomainRoute(
              name: "Vault.DeleteVault",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "Delete",
              defaults: new { area = "Vault", controller = "Vault", action = "DeleteVault" }
            );
            routes.MapSubdomainRoute(
              name: "Vault.ViewVault",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "v/{id}",
              defaults: new { area = "Vault", controller = "Vault", action = "ViewVault" }
            );
            routes.MapSubdomainRoute(
              name: "Vault.Action",
              domains: new List<string>() { config.Host },
              subDomains: new List<string>() { "vault", "v" },
              template: "Action/{action}",
              defaults: new { area = "Vault", controller = "Vault", action = "Index" }
            );
        }
    }
}
