using System.Web.Mvc;
using System.Web.Optimization;

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
            context.MapSubdomainRoute(
                 "Upload_dev",
                 "dev",
                 "Upload",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_dev_download",
                 "dev",
                 "Upload/{url}",
                 new { controller = "Upload", action = "Download", url = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_dev_delete",
                 "dev",
                 "Upload/{url}/{deleteKey}",
                 new { controller = "Upload", action = "Download", url = string.Empty, deleteKey = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_dev_action",
                 "dev",
                 "Upload/Action/{controller}/{action}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_short",
                 "u",
                 "",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_short_download",
                 "u",
                 "{url}",
                 new { controller = "Upload", action = "Download", url = "" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_short_delete",
                 "u",
                 "{url}/{deleteKey}",
                 new { controller = "Upload", action = "Download", url = string.Empty, deleteKey = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_short_action",
                 "u",
                 "Action/{controller}/{action}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_long", 
                 "upload",
                 "",  
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_long_download",
                 "upload",
                 "{url}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_long_delete",
                 "upload",
                 "{url}/{deleteKey}",
                 new { controller = "Upload", action = "Index", url = string.Empty, deleteKey = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload_default_long_action",
                 "upload",
                 "Action/{controller}/{action}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );

            // Register Script Bundles
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/upload").Include(
                      "~/Scripts/Dropzone/dropzone.js",
                      "~/Areas/Upload/Scripts/Upload.js",
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Areas/Upload/Scripts/aes.js"));
            // Register Style Bundles
            BundleTable.Bundles.Add(new StyleBundle("~/Content/upload").Include(
                      "~/Content/dropzone.css",
                      "~/Areas/Upload/Content/Upload.css"));
        }
    }
}