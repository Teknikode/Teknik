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
using Teknik.Utilities;
using Teknik.Models;
using System.Text;
using Teknik.Areas.Shortener.Models;
using nClam;
using Teknik.Filters;
using Teknik.Areas.API.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;

namespace Teknik.Areas.API.Controllers
{
    [TeknikAuthorize(AuthType.Basic)]
    public class APIv1Controller : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.API"));
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Upload(APIv1UploadModel model)
        {
            try
            {
                if (Config.UploadConfig.UploadEnabled)
                {
                    if (model.file != null)
                    {
                        if (model.file.ContentLength <= Config.UploadConfig.MaxUploadSize)
                        {
                            // convert file to bytes
                            string fileExt = Path.GetExtension(model.file.FileName);
                            int contentLength = model.file.ContentLength;

                            // Scan the file to detect a virus
                            if (Config.UploadConfig.VirusScanEnable)
                            {
                                ClamClient clam = new ClamClient(Config.UploadConfig.ClamServer, Config.UploadConfig.ClamPort);
                                clam.MaxStreamSize = Config.UploadConfig.MaxUploadSize;
                                ClamScanResult scanResult = clam.SendAndScanFile(model.file.InputStream);

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
                            if (string.IsNullOrEmpty(model.contentType))
                            {
                                model.contentType = (string.IsNullOrEmpty(model.file.ContentType)) ? "application/octet-stream" : model.file.ContentType;
                            }

                            // Initialize the key size and block size if empty
                            if (model.keySize <= 0)
                                model.keySize = Config.UploadConfig.KeySize;
                            if (model.blockSize <= 0)
                                model.blockSize = Config.UploadConfig.BlockSize;

                            // Save the file data
                            Upload.Models.Upload upload = Uploader.SaveFile(db, Config, model.file.InputStream, model.contentType, contentLength, model.encrypt, fileExt, model.iv, model.key, model.keySize, model.blockSize);

                            if (upload != null)
                            {
                                // Associate this with the user if they provided an auth key
                                if (User.Identity.IsAuthenticated)
                                {
                                    User foundUser = UserHelper.GetUser(db, User.Identity.Name);
                                    if (foundUser != null)
                                    {
                                        upload.UserId = foundUser.UserId;
                                        db.Entry(upload).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                                // Generate delete key only if asked to
                                if (!model.genDeletionKey)
                                {
                                    upload.DeleteKey = string.Empty;
                                    db.Entry(upload).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                                // Pull all the information together 
                                string fullUrl = Url.SubRouteUrl("upload", "Upload.Download", new { file = upload.Url });
                                var returnData = new
                                {
                                    url = (model.saveKey || string.IsNullOrEmpty(model.key)) ? fullUrl : fullUrl + "#" + model.key,
                                    fileName = upload.Url,
                                    contentType = model.contentType,
                                    contentLength = contentLength,
                                    key = model.key,
                                    keySize = model.keySize,
                                    iv = model.iv,
                                    blockSize = model.blockSize,
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
                return Json(new { error = new { message = "Uploads are Disabled" } });
            }
            catch(Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Paste(APIv1PasteModel model)
        {
            try
            {
                if (model != null && model.code != null)
                {
                    Paste.Models.Paste paste = PasteHelper.CreatePaste(model.code, model.title, model.syntax, model.expireUnit, model.expireLength, model.password, model.hide);

                    // Associate this with the user if they are logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        User foundUser = UserHelper.GetUser(db, User.Identity.Name);
                        if (foundUser != null)
                        {
                            paste.UserId = foundUser.UserId;
                        }
                    }

                    db.Pastes.Add(paste);
                    db.SaveChanges();

                    return Json(new
                    {
                        result = new
                        {
                            id = paste.Url,
                            url = Url.SubRouteUrl("paste", "Paste.View", new { type = "Full", url = paste.Url, password = model.password }),
                            title = paste.Title,
                            syntax = paste.Syntax,
                            expiration = paste.ExpireDate,
                            password = model.password
                        }
                    });
                }
                return Json(new { error = new { message = "Invalid Paste Request" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Shorten(APIv1ShortenModel model)
        {
            try
            {
                if (model.url.IsValidUrl())
                {
                    ShortenedUrl newUrl = Shortener.Shortener.ShortenUrl(model.url, Config.ShortenerConfig.UrlLength);

                    // Associate this with the user if they are logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        User foundUser = UserHelper.GetUser(db, User.Identity.Name);
                        if (foundUser != null)
                        {
                            newUrl.UserId = foundUser.UserId;
                        }
                    }

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
                            originalUrl = model.url
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