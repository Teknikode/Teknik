using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Optimization;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Utilities;

namespace Teknik.Areas.Upload
{
    public class UploadAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Upload";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            Config config = Config.Load();
            context.MapSubdomainRoute(
                 "Upload.Index",
                 new List<string>() { "upload", "u" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "",  
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Favicon",
                 new List<string>() { "upload", "u" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "favicon.ico",
                 new { controller = "Default", action = "Favicon" },
                 new[] { typeof(DefaultController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Download",
                 new List<string>() { "upload", "u" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{file}",
                 new { controller = "Upload", action = "Download", file = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Delete",
                 new List<string>() { "upload", "u" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "{file}/{key}",
                 new { controller = "Upload", action = "Delete", file = string.Empty, key = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Action",
                 new List<string>() { "upload", "u" }, // Subdomains
                 new List<string>() { config.Host }, // domains
                 "Action/{controller}/{action}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/upload", config.CdnHost).Include(
                      "~/Scripts/Dropzone/dropzone.js",
                      "~/Areas/Upload/Scripts/Upload.js",
                      "~/Scripts/bootstrap-switch.js",
                      "~/Scripts/bootbox/bootbox.min.js"));
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/download", config.CdnHost).Include(
                      "~/Scripts/Blob.js",
                      "~/Scripts/FileSaver.js",
                      "~/Areas/Upload/Scripts/Download.js"));
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/cryptoWorker", config.CdnHost).Include(
                      "~/Areas/Upload/Scripts/EncryptionWorker.js"));
            BundleTable.Bundles.Add(new CdnScriptBundle("~/bundles/crypto", config.CdnHost).Include(
                      "~/Scripts/Crypto-js/aes.js",
                      "~/Scripts/Crypto-js/enc-base64.js",
                      "~/Scripts/Crypto-js/mode-ctr.js",
                      "~/Scripts/Crypto-js/lib-typedarrays.js",
                      "~/Scripts/Crypto-js/pad-nopadding.js"));

            // Register Style Bundles
            BundleTable.Bundles.Add(new CdnStyleBundle("~/Content/upload", config.CdnHost).Include(
                      "~/Content/dropzone.css",
                      "~/Content/bootstrap-switch/bootstrap3/bootstrap-switch.css",
                      "~/Areas/Upload/Content/Upload.css"));
        }
    }
}