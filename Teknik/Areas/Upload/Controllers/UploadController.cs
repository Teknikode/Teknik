using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Upload.ViewModels;
using Teknik.Controllers;
using Teknik.Models;

namespace Teknik.Areas.Upload.Controllers
{
    public class UploadController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Upload/Upload
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(new UploadViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(string fileType, string iv, HttpPostedFileWrapper data)
        {
            if (data.ContentLength <= Config.UploadConfig.MaxUploadSize)
            {
                Models.Upload upload = Uploader.SaveFile(data, fileType, iv);
                if (upload != null)
                {
                    return Json(new { result = new { name = upload.Url, url = Url.SubRouteUrl("upload", "Upload.Download", new { file = upload.Url }) } }, "text/plain");
                }
                return Json(new { error = "Unable to upload file" });
            }
            else
            {
                return Json(new { error = "File Too Large" });
            }
        }

        // User did not supply key
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Download(string file)
        {
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                // We don't have the key, so we need to decrypt it client side
                if (upload.Key == null)
                {
                    DownloadViewModel model = new DownloadViewModel();
                    model.FileName = file;
                    model.ContentType = upload.ContentType;
                    model.ContentLength = upload.ContentLength;
                    model.Key = upload.Key;
                    model.IV = upload.IV;

                    return View(model);
                }
                else
                {
                    // decrypt it server side!  Weee
                    return View();
                }
            }
            else
            {
                return RedirectToRoute("Error.Http404");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public FileResult DownloadData(string file)
        {
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                string filePath = Path.Combine(Config.UploadConfig.UploadDirectory, upload.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    byte[] buffer;
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    try
                    {
                        int length = (int)fileStream.Length;  // get file length
                        buffer = new byte[length];            // create buffer
                        int count;                            // actual number of bytes read
                        int sum = 0;                          // total number of bytes read

                        // read until Read method returns 0 (end of the stream has been reached)
                        while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                            sum += count;  // sum is a buffer offset for next reading
                    }
                    finally
                    {
                        fileStream.Close();
                    }
                    return File(buffer, System.Net.Mime.MediaTypeNames.Application.Octet, file);
                }
            }
            RedirectToAction("Http404", "Error", new { area = "Errors", exception = new Exception("File Not Found") });
            return null;
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