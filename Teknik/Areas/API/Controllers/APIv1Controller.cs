using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Upload;
using Teknik.Areas.Paste;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;
using System.Text;
using Teknik.Areas.Shortener.Models;
using nClam;
using Teknik.Filters;
using Teknik.Utilities;

namespace Teknik.Areas.API.Controllers
{
    public class APIv1Controller : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.Topic", new { topic = "API" }));
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Upload(HttpPostedFileWrapper file, string contentType = null, bool encrypt = true, bool saveKey = true, string key = null, int keySize = 0, string iv = null, int blockSize = 0, bool genDeletionKey = false, bool doNotTrack = false)
        {
            try
            {
                ViewBag.Title = "Upload";
                if (file != null)
                {
                    if (file.ContentLength <= Config.UploadConfig.MaxUploadSize)
                    {
                        // convert file to bytes
                        byte[] fileData = null;
                        string fileExt = Path.GetExtension(file.FileName);
                        int contentLength = file.ContentLength;
                        using (var binaryReader = new BinaryReader(file.InputStream))
                        {
                            fileData = binaryReader.ReadBytes(file.ContentLength);
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
                                    break;
                                case ClamScanResults.Unknown:
                                    break;
                            }
                        }

                        // Need to grab the contentType if it's empty
                        if (string.IsNullOrEmpty(contentType))
                        {
                            contentType = (string.IsNullOrEmpty(file.ContentType)) ? "application/octet-stream" : file.ContentType;
                        }

                        // Initialize the key size and block size if empty
                        if (keySize <= 0)
                            keySize = Config.UploadConfig.KeySize;
                        if (blockSize <= 0)
                            blockSize = Config.UploadConfig.BlockSize;

                        byte[] data = null;
                        // If they want us to encrypt the file first, do that here
                        if (encrypt)
                        {
                            // Generate key and iv if empty
                            if (string.IsNullOrEmpty(key))
                            {
                                key = StringHelper.RandomString(keySize / 8);
                            }
                            if (string.IsNullOrEmpty(iv))
                            {
                                iv = StringHelper.RandomString(blockSize / 8);
                            }

                            data = AES.Encrypt(fileData, key, iv);
                            if (data == null || data.Length <= 0)
                            {
                                return Json(new { error = new { message = "Unable to encrypt file" } });
                            }
                        }

                        // Save the file data
                        Upload.Models.Upload upload = Uploader.SaveFile(db, Config, (encrypt) ? data : fileData, contentType, contentLength, fileExt, iv, (saveKey) ? key : null, keySize, blockSize);

                        if (upload != null)
                        {
                            // Generate delete key if asked to
                            if (genDeletionKey)
                            {
                                string delKey = StringHelper.RandomString(Config.UploadConfig.DeleteKeyLength);
                                upload.DeleteKey = delKey;
                                db.Entry(upload).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            // Pull all the information together 
                            string fullUrl = Url.SubRouteUrl("upload", "Upload.Download", new { file = upload.Url });
                            var returnData = new
                            {
                                url = (saveKey || string.IsNullOrEmpty(key)) ? fullUrl : fullUrl + "#" + key,
                                fileName = upload.Url,
                                contentType = contentType,
                                contentLength = contentLength,
                                key = key,
                                keySize = keySize,
                                iv = iv,
                                blockSize = blockSize,
                                deletionKey = upload.DeleteKey

                            };
                            return Json(new { result = returnData });
                        }
                        return Json(new { error = new { message = "Unable to save file" } });
                    }
                    else
                    {
                        return Json(new { error = new { message = "File Too Large" } });
                    }
                }

                return Json(new { error = new { message = "Invalid Upload Request" } });
            }
            catch(Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Paste(string code, string title = "", string syntax = "text", string expireUnit = "never", int expireLength = 1, string password = "", bool hide = false, bool doNotTrack = false)
        {
            try
            {
                ViewBag.Title = "Paste";
                Paste.Models.Paste paste = PasteHelper.CreatePaste(code, title, syntax, expireUnit, expireLength, password, hide);

                db.Pastes.Add(paste);
                db.SaveChanges();

                return Json(new
                {
                    result = new
                    {
                        id = paste.Url,
                        url = Url.SubRouteUrl("paste", "Paste.View", new { type = "Full", url = paste.Url, password = password }),
                        title = paste.Title,
                        syntax = paste.Syntax,
                        expiration = paste.ExpireDate,
                        password = password
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Shorten(string url, bool doNotTrack = false)
        {
            try
            {
                ViewBag.Title = "Shorten";
                if (url.IsValidUrl())
                {
                    ShortenedUrl newUrl = Shortener.Shortener.ShortenUrl(url, Config.ShortenerConfig.UrlLength);

                    db.ShortenedUrls.Add(newUrl);
                    db.SaveChanges();

                    string shortUrl = string.Format("{0}://{1}/{2}", HttpContext.Request.Url.Scheme, Config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl);
                    if (Config.DevEnvironment)
                    {
                        shortUrl = Url.SubRouteUrl("shortened", "Shortener.View", new { url = newUrl.ShortUrl });
                    }

                    return Json(new
                    {
                        result = new
                        {
                            shortUrl = shortUrl,
                            originalUrl = url
                        }
                    });
                }
                return Json(new { error = new { message = "Must be a valid Url" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }
    }
}