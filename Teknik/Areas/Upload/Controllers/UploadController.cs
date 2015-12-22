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
        public ActionResult Upload(string uploadID)
        {
            return Json(new { result = "tempURL.png" });
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