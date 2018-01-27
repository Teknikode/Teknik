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
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Attributes;
using System.Text;
using Teknik.Utilities.Cryptography;

namespace Teknik.Areas.Upload.Controllers
{
    [TeknikAuthorize]
    public class UploadController : DefaultController
    {
        // GET: Upload/Upload
        [HttpGet]
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Teknik Upload - End to End Encryption";
            UploadViewModel model = new UploadViewModel();
            model.CurrentSub = Subdomain;
            using (TeknikEntities db = new TeknikEntities())
            {
                Users.Models.User user = UserHelper.GetUser(db, User.Identity.Name);
                if (user != null)
                {
                    model.Encrypt = user.UploadSettings.Encrypt;
                    model.Vaults = user.Vaults.ToList();
                }
                else
                {
                    model.Encrypt = false;
                }
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Upload(string fileType, string fileExt, string iv, int keySize, int blockSize, bool encrypt, HttpPostedFileWrapper data)
        {
            try
            {
                if (Config.UploadConfig.UploadEnabled)
                {
                    if (data.ContentLength <= Config.UploadConfig.MaxUploadSize)
                    {
                        // convert file to bytes
                        int contentLength = data.ContentLength;

                        // Scan the file to detect a virus
                        if (Config.UploadConfig.VirusScanEnable)
                        {
                            ClamClient clam = new ClamClient(Config.UploadConfig.ClamServer, Config.UploadConfig.ClamPort);
                            clam.MaxStreamSize = Config.UploadConfig.MaxUploadSize;
                            ClamScanResult scanResult = clam.SendAndScanFile(data.InputStream);

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

                        // Check content type restrictions (Only for encrypting server side
                        if (encrypt)
                        {
                            if (Config.UploadConfig.RestrictedContentTypes.Contains(fileType))
                            {
                                return Json(new { error = new { message = "File Type Not Allowed" } });
                            }
                        }

                        using (TeknikEntities db = new TeknikEntities())
                        {
                            Models.Upload upload = Uploader.SaveFile(db, Config, data.InputStream, fileType, contentLength, encrypt, fileExt, iv, null, keySize, blockSize);
                            if (upload != null)
                            {
                                if (User.Identity.IsAuthenticated)
                                {
                                    Users.Models.User user = UserHelper.GetUser(db, User.Identity.Name);
                                    if (user != null)
                                    {
                                        upload.UserId = user.UserId;
                                        db.Entry(upload).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                return Json(new { result = new { name = upload.Url, url = Url.SubRouteUrl("u", "Upload.Download", new { file = upload.Url }), contentType = upload.ContentType, contentLength = StringHelper.GetBytesReadable(upload.ContentLength), deleteUrl = Url.SubRouteUrl("u", "Upload.Delete", new { file = upload.Url, key = upload.DeleteKey }) } }, "text/plain");
                            }
                            return Json(new { error = new { message = "Unable to upload file" } });
                        }
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
        [TrackDownload]
        [AllowAnonymous]
        public ActionResult Download(string file)
        {
            if (Config.UploadConfig.DownloadEnabled)
            {
                ViewBag.Title = "Teknik Download - " + file;
                string fileName = string.Empty;
                string url = string.Empty;
                string key = string.Empty;
                string iv = string.Empty;
                string contentType = string.Empty;
                long contentLength = 0;
                DateTime dateUploaded = new DateTime();

                using (TeknikEntities db = new TeknikEntities())
                {
                    Models.Upload uploads = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
                    if (uploads != null)
                    {
                        uploads.Downloads += 1;
                        db.Entry(uploads).State = EntityState.Modified;
                        db.SaveChanges();

                        fileName = uploads.FileName;
                        url = uploads.Url;
                        key = uploads.Key;
                        iv = uploads.IV;
                        contentType = uploads.ContentType;
                        contentLength = uploads.ContentLength;
                        dateUploaded = uploads.DateUploaded;
                    }
                    else
                    {
                        return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
                    }
                }

                // We don't have the key, so we need to decrypt it client side
                if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
                {
                    DownloadViewModel model = new DownloadViewModel();
                    model.FileName = file;
                    model.ContentType = contentType;
                    model.ContentLength = contentLength;
                    model.IV = iv;

                    return View(model);
                }
                else // We have the key, so that means server side decryption
                {
                    // Check for the cache
                    bool isCached = false;
                    string modifiedSince = Request.Headers["If-Modified-Since"];
                    if (!string.IsNullOrEmpty(modifiedSince))
                    {
                        DateTime modTime = new DateTime();
                        bool parsed = DateTime.TryParse(modifiedSince, out modTime);
                        if (parsed)
                        {
                            if ((modTime - dateUploaded).TotalSeconds <= 1)
                            {
                                isCached = true;
                            }
                        }
                    }

                    if (isCached)
                    {
                        // The file is cached, let's just 304 this
                        Response.StatusCode = 304;
                        Response.StatusDescription = "Not Modified";
                        return new EmptyResult();
                    }
                    else
                    {
                        string subDir = fileName[0].ToString();
                        string filePath = Path.Combine(Config.UploadConfig.UploadDirectory, subDir, fileName);
                        long startByte = 0;
                        long endByte = contentLength - 1;
                        long length = contentLength;
                        if (System.IO.File.Exists(filePath))
                        {
                            #region Range Calculation
                            // Are they downloading it by range?
                            bool byRange = !string.IsNullOrEmpty(Request.ServerVariables["HTTP_RANGE"]); // We do not support ranges

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
                                    Response.AddHeader("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);
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
                                    anotherEnd = (arr_split.Length > 1 && Int64.TryParse(arr_split[1].ToString(), out temp)) ? Convert.ToInt64(arr_split[1]) : contentLength;
                                }

                                /* Check the range and make sure it's treated according to the specs. 
                                 * http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html 
                                 */
                                // End bytes can not be larger than $end. 
                                anotherEnd = (anotherEnd > endByte) ? endByte : anotherEnd;
                                // Validate the requested range and return an error if it's not correct. 
                                if (anotherStart > anotherEnd || anotherStart > contentLength - 1 || anotherEnd >= contentLength)
                                {

                                    Response.AddHeader("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);
                                    throw new HttpException(416, "Requested Range Not Satisfiable");
                                }
                                startByte = anotherStart;
                                endByte = anotherEnd;

                                length = endByte - startByte + 1; // Calculate new content length 

                                // Ranges are response of 206
                                Response.StatusCode = 206;
                            }
                            #endregion

                            // Add cache parameters
                            Response.Cache.SetCacheability(HttpCacheability.Public);
                            Response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
                            Response.Cache.SetLastModified(dateUploaded);

                            // We accept ranges
                            Response.AddHeader("Accept-Ranges", "0-" + contentLength);

                            // Notify the client the byte range we'll be outputting 
                            Response.AddHeader("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                            // Notify the client the content length we'll be outputting 
                            Response.AddHeader("Content-Length", length.ToString());

                            // Create content disposition
                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                FileName = url,
                                Inline = true
                            };

                            Response.AddHeader("Content-Disposition", cd.ToString());

                            // Apply content security policy for downloads
                            //Response.AddHeader("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self'; font-src *; connect-src 'self'; media-src 'self'; child-src 'self'; form-action 'none';");

                            // Read in the file
                            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                            // Reset file stream to starting position (or start of range)
                            fs.Seek(startByte, SeekOrigin.Begin);

                            try
                            {
                                // If the IV is set, and Key is set, then decrypt it while sending
                                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
                                {
                                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                                    byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

                                    return new FileGenerateResult(url,
                                                                contentType,
                                                                (response) => ResponseHelper.StreamToOutput(response, true, new AesCounterStream(fs, false, keyBytes, ivBytes), (int)length, Config.UploadConfig.ChunkSize),
                                                                false);
                                }
                                else // Otherwise just send it
                                {
                                    // Send the file
                                    return new FileGenerateResult(url,
                                                                contentType,
                                                                (response) => ResponseHelper.StreamToOutput(response, true, fs, (int)length, Config.UploadConfig.ChunkSize),
                                                                false);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Logger.WriteEntry(Logging.LogLevel.Warning, "Error in Download", ex);
                            }
                        }
                    }
                    return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpPost]
        [AllowAnonymous]
        public FileResult DownloadData(string file)
        {
            if (Config.UploadConfig.DownloadEnabled)
            {
                using (TeknikEntities db = new TeknikEntities())
                {
                    Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
                    if (upload != null)
                    {
                        string subDir = upload.FileName[0].ToString();
                        string filePath = Path.Combine(Config.UploadConfig.UploadDirectory, subDir, upload.FileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                            return File(fileStream, System.Net.Mime.MediaTypeNames.Application.Octet, file);
                        }
                    }
                    Redirect(Url.SubRouteUrl("error", "Error.Http404"));
                    return null;
                }
            }
            Redirect(Url.SubRouteUrl("error", "Error.Http403"));
            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Delete(string file, string key)
        {
            using (TeknikEntities db = new TeknikEntities())
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
        }

        [HttpPost]
        public ActionResult GenerateDeleteKey(string file)
        {
            using (TeknikEntities db = new TeknikEntities())
            {
                Models.Upload upload = db.Uploads.Where(up => up.Url == file).FirstOrDefault();
                if (upload != null)
                {
                    if (upload.User.Username == User.Identity.Name)
                    {
                        string delKey = StringHelper.RandomString(Config.UploadConfig.DeleteKeyLength);
                        upload.DeleteKey = delKey;
                        db.Entry(upload).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = new { url = Url.SubRouteUrl("u", "Upload.Delete", new { file = file, key = delKey }) } });
                    }
                    return Json(new { error = new { message = "You do not own this upload" } });
                }
                return Json(new { error = new { message = "Invalid URL" } });
            }
        }
    }
}
