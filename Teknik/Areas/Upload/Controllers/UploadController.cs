using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Error.ViewModels;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Upload.ViewModels;
using Teknik.Controllers;
using Teknik.Helpers;
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
            ViewBag.Title = "Teknik Upload - End to End Encryption";
            return View(new UploadViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(string fileType, string iv, int keySize, int blockSize, HttpPostedFileWrapper data)
        {
            if (data.ContentLength <= Config.UploadConfig.MaxUploadSize)
            {
                Models.Upload upload = Uploader.SaveFile(data, fileType, iv, null, keySize, blockSize);
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
            ViewBag.Title = "Teknik Download - " + file;
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                // We don't have the key, so we need to decrypt it client side
                if (string.IsNullOrEmpty(upload.Key))
                {
                    DownloadViewModel model = new DownloadViewModel();
                    model.FileName = file;
                    model.ContentType = upload.ContentType;
                    model.ContentLength = upload.ContentLength;
                    model.IV = upload.IV;

                    return View(model);
                }
                else // We have the key, so that means server side decryption
                {
                    if (System.IO.File.Exists(upload.FileName))
                    {
                        // Read in the file
                        byte[] encData = System.IO.File.ReadAllBytes(upload.FileName);
                        // Decrypt the data
                        byte[] data = AES.Decrypt(encData, upload.Key, upload.IV, upload.KeySize, upload.BlockSize);

                        // Create File
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            FileName = upload.Url,
                            Inline = true
                        };

                        Response.AppendHeader("Content-Disposition", cd.ToString());

                        return File(data, upload.ContentType);
                    }
                }
            }
            return RedirectToRoute("*.Error.Http404");
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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Delete(string file, string key)
        {
            ViewBag.Title = "File Delete - " + file + " - " + Config.Title;
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                DeleteViewModel model = new DeleteViewModel();
                model.File = file;
                if (!string.IsNullOrEmpty(upload.DeleteKey) && upload.DeleteKey == key)
                {
                    string filePath = upload.FileName;
                    // Delete from the DB
                    db.Uploads.Remove(upload);
                    db.SaveChanges();

                    // Delete the File
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    model.Deleted = true;
                }
                else
                {
                    model.Deleted = false;
                }
                return View(model);
            }
            return RedirectToRoute("*.Error.Http404");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateDeleteKey(string file)
        {
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                string delKey = Utility.RandomString(Config.UploadConfig.DeleteKeyLength);
                upload.DeleteKey = delKey;
                db.Entry(upload).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { result = Url.SubRouteUrl("upload", "Upload.Delete", new { file = file, key = delKey }) });
            }
            return Json(new { error = "Invalid URL" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFileKey(string file, string key)
        {
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                upload.Key = key;
                db.Entry(upload).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { result = Url.SubRouteUrl("upload", "Upload.Download", new { file = file }) });
            }
            return Json(new { error = "Invalid URL" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFileKey(string file, string key)
        {
            Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                if (upload.Key == key)
                {
                    upload.Key = null;
                    db.Entry(upload).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = Url.SubRouteUrl("upload", "Upload.Download", new { file = file }) });
                }
                return Json(new { error = "Non-Matching Key" });
            }
            return Json(new { error = "Invalid URL" });
        }
    }
}