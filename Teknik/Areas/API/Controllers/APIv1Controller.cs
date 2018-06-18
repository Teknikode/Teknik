using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Teknik.Areas.Upload;
using Teknik.Areas.Paste;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using System.Text;
using MimeDetective;
using MimeDetective.Extensions;
using Teknik.Areas.Shortener.Models;
using nClam;
using Teknik.Filters;
using Teknik.Areas.API.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Teknik.Areas.Shortener;
using Teknik.Logging;

namespace Teknik.Areas.API.Controllers
{
    [TeknikAuthorize(AuthType.Basic)]
    [Area("APIv1")]
    public class APIv1Controller : DefaultController
    {
        public APIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.API"));
        }

        [HttpPost]
        [AllowAnonymous]
        [ServiceFilter(typeof(TrackPageView))]
        public async Task<IActionResult> UploadAsync(APIv1UploadModel model)
        {
            try
            {
                if (_config.UploadConfig.UploadEnabled)
                {
                    if (model.file != null)
                    {
                        long maxUploadSize = _config.UploadConfig.MaxUploadSize;
                        if (User.Identity.IsAuthenticated)
                        {
                            maxUploadSize = _config.UploadConfig.MaxUploadSizeBasic;
                            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                            if (user.AccountType == AccountType.Premium)
                            {
                                maxUploadSize = _config.UploadConfig.MaxUploadSizePremium;
                            }
                        }
                        if (model.file.Length <= maxUploadSize)
                        {
                            // convert file to bytes
                            string fileExt = Path.GetExtension(model.file.FileName);
                            long contentLength = model.file.Length;

                            // Scan the file to detect a virus
                            if (_config.UploadConfig.VirusScanEnable)
                            {
                                ClamClient clam = new ClamClient(_config.UploadConfig.ClamServer, _config.UploadConfig.ClamPort);
                                clam.MaxStreamSize = maxUploadSize;
                                ClamScanResult scanResult = await clam.SendAndScanFileAsync(model.file.OpenReadStream());

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
                                model.contentType = model.file.ContentType;

                                if (string.IsNullOrEmpty(model.contentType))
                                {
                                    using (System.IO.Stream fileStream = model.file.OpenReadStream())
                                    {
                                        fileStream.Seek(0, SeekOrigin.Begin);
                                        FileType fileType = fileStream.GetFileType();
                                        if (fileType != null)
                                            model.contentType = fileType.Mime;
                                        if (string.IsNullOrEmpty(model.contentType))
                                        {
                                            model.contentType = "application/octet-stream";
                                        }
                                    }
                                }
                            }

                            // Check content type restrictions (Only for encrypting server side
                            if (model.encrypt || !string.IsNullOrEmpty(model.key))
                            {
                                if (_config.UploadConfig.RestrictedContentTypes.Contains(model.contentType) || _config.UploadConfig.RestrictedExtensions.Contains(fileExt))
                                {
                                    return Json(new { error = new { message = "File Type Not Allowed" } });
                                }
                            }

                            // Initialize the key size and block size if empty
                            if (model.keySize <= 0)
                                model.keySize = _config.UploadConfig.KeySize;
                            if (model.blockSize <= 0)
                                model.blockSize = _config.UploadConfig.BlockSize;

                            // Save the file data
                            Upload.Models.Upload upload = Uploader.SaveFile(_dbContext, _config, model.file.OpenReadStream(), model.contentType, contentLength, model.encrypt, fileExt, model.iv, model.key, model.keySize, model.blockSize);

                            if (upload != null)
                            {
                                string fileKey = upload.Key;

                                // Associate this with the user if they provided an auth key
                                if (User.Identity.IsAuthenticated)
                                {
                                    User foundUser = UserHelper.GetUser(_dbContext, User.Identity.Name);
                                    if (foundUser != null)
                                    {
                                        upload.UserId = foundUser.UserId;
                                        _dbContext.Entry(upload).State = EntityState.Modified;
                                        _dbContext.SaveChanges();
                                    }
                                }

                                // Generate delete key only if asked to
                                if (!model.genDeletionKey)
                                {
                                    upload.DeleteKey = string.Empty;
                                    _dbContext.Entry(upload).State = EntityState.Modified;
                                    _dbContext.SaveChanges();
                                }

                                // remove the key if we don't want to save it
                                if (!model.saveKey)
                                {
                                    upload.Key = null;
                                    _dbContext.Entry(upload).State = EntityState.Modified;
                                    _dbContext.SaveChanges();
                                }

                                // Pull all the information together 
                                string fullUrl = Url.SubRouteUrl("u", "Upload.Download", new { file = upload.Url });
                                var returnData = new
                                {
                                    url = (model.saveKey || string.IsNullOrEmpty(fileKey)) ? fullUrl : fullUrl + "#" + fileKey,
                                    fileName = upload.Url,
                                    contentType = upload.ContentType,
                                    contentLength = upload.ContentLength,
                                    key = fileKey,
                                    keySize = upload.KeySize,
                                    iv = upload.IV,
                                    blockSize = upload.BlockSize,
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
        [ServiceFilter(typeof(TrackPageView))]
        public IActionResult Paste(APIv1PasteModel model)
        {
            try
            {
                if (model != null && model.code != null)
                {
                    Paste.Models.Paste paste = PasteHelper.CreatePaste(_config, _dbContext, model.code, model.title, model.syntax, model.expireUnit, model.expireLength, model.password, model.hide);

                    // Associate this with the user if they are logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        User foundUser = UserHelper.GetUser(_dbContext, User.Identity.Name);
                        if (foundUser != null)
                        {
                            paste.UserId = foundUser.UserId;
                        }
                    }

                    _dbContext.Pastes.Add(paste);
                    _dbContext.SaveChanges();

                    return Json(new
                    {
                        result = new
                        {
                            id = paste.Url,
                            url = Url.SubRouteUrl("p", "Paste.View", new { type = "Full", url = paste.Url, password = model.password }),
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
        [ServiceFilter(typeof(TrackPageView))]
        public IActionResult Shorten(APIv1ShortenModel model)
        {
            try
            {
                if (model.url.IsValidUrl())
                {
                    ShortenedUrl newUrl = ShortenerHelper.ShortenUrl(_dbContext, model.url, _config.ShortenerConfig.UrlLength);

                    // Associate this with the user if they are logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        User foundUser = UserHelper.GetUser(_dbContext, User.Identity.Name);
                        if (foundUser != null)
                        {
                            newUrl.UserId = foundUser.UserId;
                        }
                    }

                    _dbContext.ShortenedUrls.Add(newUrl);
                    _dbContext.SaveChanges();

                    string shortUrl = string.Format("{0}://{1}/{2}", HttpContext.Request.Scheme, _config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl);
                    if (_config.DevEnvironment)
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
