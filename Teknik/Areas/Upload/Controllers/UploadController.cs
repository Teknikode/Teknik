using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Upload.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Upload.Controllers
{
    public class UploadController : DefaultController
    {
        // GET: Upload/Upload
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(new UploadViewModel());
        }
        // GET: Upload/Upload
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Download(string url)
        {
            return View(new UploadViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(string iv)
        {
            Models.Upload upload = null;
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];
                //Save file content goes here
                string fName = file.FileName;
                if (file != null && file.ContentLength > 0)
                {
                    upload = Uploader.SaveFile(file, iv);
                    break;
                }
            }
            if (upload != null)
            {
                return Json(new { result = new { name = upload.Url, url = Url.SubRouteUrl("upload", "Upload.Download", new { file = upload.Url }) } }, "text/plain");
            }
            return Json(new { error = "Unable to upload file" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string url, string deleteKey)
        {
            return Json(new { result = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateDeleteKey(string uploadID)
        {
            return Json(new { result = "temp-delete-key" });
        }
    }
}