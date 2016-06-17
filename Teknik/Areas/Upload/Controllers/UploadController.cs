using nClam;
using Piwik.Tracker;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Error.ViewModels;
using Teknik.Areas.Upload.Models;
using Teknik.Areas.Upload.ViewModels;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.Upload.Controllers
{
    public class UploadController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Upload/Upload
        [HttpGet]
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Teknik Upload - End to End Encryption";
            UploadViewModel model = new UploadViewModel();
            model.CurrentSub = Subdomain;
            Users.Models.User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            if (user != null)
            {
                model.SaveKey = user.UploadSettings.SaveKey;
                model.ServerSideEncrypt = user.UploadSettings.ServerSideEncrypt;
            }
            else
            {
                model.SaveKey = true;
                model.ServerSideEncrypt = true;
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Upload(string fileType, string fileExt, string iv, int keySize, int blockSize, bool encrypt, bool saveKey, HttpPostedFileWrapper data, string key = null)
        {
            try
            {
                if (Config.UploadConfig.UploadEnabled)
                {
                    if (data.ContentLength <= Config.UploadConfig.MaxUploadSize)
                    {
                        // convert file to bytes
                        byte[] fileData = null;
                        int contentLength = data.ContentLength;
                        using (var binaryReader = new BinaryReader(data.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(data.ContentLength);
                        }

                        // Scan the file to detect a virus
                        if (Config.UploadConfig.VirusScanEnable)
                        {
                            byte[] scanData = fileData;
                            // If it was encrypted client side, decrypt it
                            if (!encrypt && key != null)
                            {
                                // If the IV is set, and Key is set, then decrypt it
                                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
                                {
                                    // Decrypt the data
                                    scanData = AES.Decrypt(scanData, key, iv);
                                }
                            }
                            ClamClient clam = new ClamClient(Config.UploadConfig.ClamServer, Config.UploadConfig.ClamPort);
                            clam.MaxStreamSize = Config.UploadConfig.MaxUploadSize;
                            ClamScanResult scanResult = clam.SendAndScanFile(scanData);

                            switch (scanResult.Result)
                            {
                                case ClamScanResults.Clean:
                                    break;
                                case ClamScanResults.VirusDetected:
                                    return Json(new { error = new { message = string.Format("Virus Detected: {0}. As per our <a href=\"{1}\">Terms of Service</a>, Viruses are not permited.", scanResult.InfectedFiles.First().VirusName, Url.SubRouteUrl("tos", "TOS.Index")) } });
                                case ClamScanResults.Error:
                                    return Json(new { error = new { message = string.Format("Error scanning the file upload for viruses.  {0}", scanResult.RawResult) } });
                                case ClamScanResults.Unknown:
                                    return Json(new { error = new { message = string.Format("Unknown result while scanning the file upload for viruses.  {0}", scanResult.RawResult) } });
                            }
                        }

                        // if they want us to encrypt it, we do so here
                        if (encrypt)
                        {
                            // Generate key and iv if empty
                            if (string.IsNullOrEmpty(key))
                            {
                                key = Utility.RandomString(keySize / 8);
                            }

                            fileData = AES.Encrypt(fileData, key, iv);
                            if (fileData == null || fileData.Length <= 0)
                            {
                                return Json(new { error = new { message = "Unable to encrypt file" } });
                            }
                        }
                        Models.Upload upload = Uploader.SaveFile(fileData, fileType, contentLength, fileExt, iv, (saveKey) ? key : null, keySize, blockSize);
                        if (upload != null)
                        {
                            if (User.Identity.IsAuthenticated)
                            {
                                Users.Models.User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                                if (user != null)
                                {
                                    upload.UserId = user.UserId;
                                    db.Entry(upload).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            return Json(new { result = new { name = upload.Url, url = Url.SubRouteUrl("u", "Upload.Download", new { file = upload.Url }), key = key } }, "text/plain");
                        }
                        return Json(new { error = new { message = "Unable to upload file" } });
                    }
                    else
                    {
                        return Json(new { error = new { message = "File Too Large" } });
                    }
                }
                return Json(new { error = new { message = "Uploads are disabled" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception while uploading file: " + ex.GetFullMessage(true) } });
            }
        }

        // User did not supply key
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Download(string file)
        {
            if (Config.UploadConfig.DownloadEnabled)
            {
                ViewBag.Title = "Teknik Download - " + file;
                Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
                if (upload != null)
                {
                    upload.Downloads += 1;
                    db.Entry(upload).State = EntityState.Modified;
                    db.SaveChanges();

                    // We don't have the key, so we need to decrypt it client side
                    if (string.IsNullOrEmpty(upload.Key) && !string.IsNullOrEmpty(upload.IV))
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
                        // Are they downloading it by range?
                        bool byRange = !string.IsNullOrEmpty(Request.ServerVariables["HTTP_RANGE"]);
                        // Check to see if they have a cache
                        bool isCached = !string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]);

                        if (isCached)
                        {
                            // The file is cached, let's just 304 this
                            Response.StatusCode = 304;
                            Response.StatusDescription = "Not Modified";
                            Response.AddHeader("Content-Length", "0");
                            return Content(string.Empty);
                        }
                        else
                        {
                            string subDir = upload.FileName[0].ToString();
                            string filePath = Path.Combine(Config.UploadConfig.UploadDirectory, subDir, upload.FileName);
                            long startByte = 0;
                            long endByte = upload.ContentLength - 1;
                            long length = upload.ContentLength;
                            if (System.IO.File.Exists(filePath))
                            {
                                // Read in the file
                                byte[] data = System.IO.File.ReadAllBytes(filePath);
                                // If the IV is set, and Key is set, then decrypt it
                                if (!string.IsNullOrEmpty(upload.Key) && !string.IsNullOrEmpty(upload.IV))
                                {
                                    // Decrypt the data
                                    data = AES.Decrypt(data, upload.Key, upload.IV);
                                }

                                // We accept ranges
                                Response.AddHeader("Accept-Ranges", "0-" + upload.ContentLength);

                                // check to see if we need to pass a specified range
                                if (byRange)
                                {
                                    long anotherStart = startByte;
                                    long anotherEnd = endByte;
                                    string[] arr_split = Request.ServerVariables["HTTP_RANGE"].Split(new char[] { '=' });
                                    string range = arr_split[1];

                                    // Make sure the client hasn't sent us a multibyte range 
                                    if (range.IndexOf(",") > -1)
                                    {
                                        // (?) Shoud this be issued here, or should the first 
                                        // range be used? Or should the header be ignored and 
                                        // we output the whole content? 
                                        Response.AddHeader("Content-Range", "bytes " + startByte + "-" + endByte + "/" + upload.ContentLength);
                                        throw new HttpException(416, "Requested Range Not Satisfiable");

                                    }

                                    // If the range starts with an '-' we start from the beginning 
                                    // If not, we forward the file pointer 
                                    // And make sure to get the end byte if spesified 
                                    if (range.StartsWith("-"))
                                    {
                                        // The n-number of the last bytes is requested 
                                        anotherStart = startByte - Convert.ToInt64(range.Substring(1));
                                    }
                                    else
                                    {
                                        arr_split = range.Split(new char[] { '-' });
                                        anotherStart = Convert.ToInt64(arr_split[0]);
                                        long temp = 0;
                                        anotherEnd = (arr_split.Length > 1 && Int64.TryParse(arr_split[1].ToString(), out temp)) ? Convert.ToInt64(arr_split[1]) : upload.ContentLength;
                                    }

                                    /* Check the range and make sure it's treated according to the specs. 
                                     * http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html 
                                     */
                                    // End bytes can not be larger than $end. 
                                    anotherEnd = (anotherEnd > endByte) ? endByte : anotherEnd;
                                    // Validate the requested range and return an error if it's not correct. 
                                    if (anotherStart > anotherEnd || anotherStart > upload.ContentLength - 1 || anotherEnd >= upload.ContentLength)
                                    {

                                        Response.AddHeader("Content-Range", "bytes " + startByte + "-" + endByte + "/" + upload.ContentLength);
                                        throw new HttpException(416, "Requested Range Not Satisfiable");
                                    }
                                    startByte = anotherStart;
                                    endByte = anotherEnd;

                                    length = endByte - startByte + 1; // Calculate new content length 

                                    // grab the portion of the data we want
                                    byte[] dataRange = new byte[length];
                                    Array.Copy(data, startByte, dataRange, 0, length);
                                    data = dataRange;

                                    // Ranges are response of 206
                                    Response.StatusCode = 206;
                                }

                                // Add cache parameters
                                Response.Cache.SetCacheability(HttpCacheability.Public);
                                Response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
                                Response.Cache.SetLastModified(upload.DateUploaded);

                                // Notify the client the byte range we'll be outputting 
                                Response.AddHeader("Content-Range", "bytes " + startByte + "-" + endByte + "/" + upload.ContentLength);
                                Response.AddHeader("Content-Length", length.ToString());

                                // Create content disposition
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
                }
                return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpPost]
        [AllowAnonymous]
        public FileResult DownloadData(string file)
        {
            if (Config.UploadConfig.DownloadEnabled)
            {
                Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
                if (upload != null)
                {
                    string subDir = upload.FileName[0].ToString();
                    string filePath = Path.Combine(Config.UploadConfig.UploadDirectory, subDir, upload.FileName);
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
                Redirect(Url.SubRouteUrl("error", "Error.Http404"));
                return null;
            }
            Redirect(Url.SubRouteUrl("error", "Error.Http403"));
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
            return RedirectToRoute("Error.Http404");
        }

        [HttpPost]
        [AllowAnonymous]
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