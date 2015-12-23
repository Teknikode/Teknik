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
                 "Upload.Index",
                 "dev",
                 "Upload",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Download",
                 "dev",
                 "Upload/{url}",
                 new { controller = "Upload", action = "Download", url = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Delete",
                 "dev",
                 "Upload/{url}/{deleteKey}",
                 new { controller = "Upload", action = "Delete", url = string.Empty, deleteKey = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Action",
                 "dev",
                 "Upload/Action/{controller}/{action}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Index",
                 "u",
                 "",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Download",
                 "u",
                 "{url}",
                 new { controller = "Upload", action = "Download", url = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Delete",
                 "u",
                 "{url}/{deleteKey}",
                 new { controller = "Upload", action = "Delete", url = string.Empty, deleteKey = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Action",
                 "u",
                 "Action/{controller}/{action}",
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Index", 
                 "upload",
                 "",  
                 new { controller = "Upload", action = "Index" },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Download",
                 "upload",
                 "{url}",
                 new { controller = "Upload", action = "Download", url = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Delete",
                 "upload",
                 "{url}/{deleteKey}",
                 new { controller = "Upload", action = "Delete", url = string.Empty, deleteKey = string.Empty },
                 new[] { typeof(Controllers.UploadController).Namespace }
             );
            context.MapSubdomainRoute(
                 "Upload.Action",
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