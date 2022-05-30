using System;
using System.IO;
using System.Linq;
using Teknik.Areas.Upload.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Attributes;
using System.Text;
using Teknik.Utilities.Cryptography;
using Teknik.Data;
using Teknik.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Teknik.Logging;
using Teknik.Areas.Users.Models;
using Teknik.ContentScanningService;
using Teknik.Utilities.Routing;
using Teknik.StorageService;

namespace Teknik.Areas.Upload.Controllers
{
    [Authorize]
    [Area("Upload")]
    public class UploadController : DefaultController
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly ObjectCache _cache;

        public UploadController(ILogger<Logger> logger, Config config, TeknikEntities dbContext, IBackgroundTaskQueue queue, ObjectCache cache) : base(logger, config, dbContext)
        {
            _queue = queue;
            _cache = cache;
        }

        [HttpGet]
        [AllowAnonymous]
        [TrackPageView]
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Upload Files";
            UploadViewModel model = new UploadViewModel();
            model.CurrentSub = Subdomain;
            model.Encrypt = false;
            model.ExpirationLength = 1;
            model.ExpirationUnit = ExpirationUnit.Days;
            model.MaxUploadSize = _config.UploadConfig.MaxUploadFileSize;
            if (User.Identity.IsAuthenticated)
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    model.Encrypt = user.UploadSettings.Encrypt;
                    model.ExpirationLength = user.UploadSettings.ExpirationLength;
                    model.ExpirationUnit = user.UploadSettings.ExpirationUnit;
                    model.Vaults = user.Vaults.ToList();

                    model.CurrentTotalSize = user.Uploads.Sum(u => u.ContentLength);

                    model.MaxUploadSize = _config.UploadConfig.MaxUploadFileSize;
                    model.MaxTotalSize = _config.UploadConfig.MaxStorage;
                    if (user.UploadSettings.MaxUploadStorage != null)
                        model.MaxTotalSize = user.UploadSettings.MaxUploadStorage.Value;
                    if (user.UploadSettings.MaxUploadFileSize != null)
                        model.MaxUploadSize = user.UploadSettings.MaxUploadFileSize.Value;

                    if (model.CurrentTotalSize >= model.MaxTotalSize)
                    {
                        model.Error = true;
                        model.ErrorMessage = string.Format("Account storage limit exceeded: {0} / {1}", StringHelper.GetBytesReadable(model.CurrentTotalSize), StringHelper.GetBytesReadable(model.MaxTotalSize));
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromForm] UploadFileViewModel uploadFile)
        {
            try
            {
                if (_config.UploadConfig.UploadEnabled)
                {
                    long maxUploadSize = _config.UploadConfig.MaxUploadFileSize;
                    long maxTotalSize = _config.UploadConfig.MaxStorage;
                    if (User.Identity.IsAuthenticated)
                    {
                        // Check account total limits
                        var user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                        if (user.UploadSettings.MaxUploadStorage != null)
                            maxTotalSize = user.UploadSettings.MaxUploadStorage.Value;
                        if (user.UploadSettings.MaxUploadFileSize != null)
                            maxUploadSize = user.UploadSettings.MaxUploadFileSize.Value;

                        var userUploadSize = user.Uploads.Sum(u => u.ContentLength);
                        if (userUploadSize + uploadFile.file.Length > maxTotalSize)
                        {
                            return Json(new { error = new { message = string.Format("Account storage limit exceeded.  {0} / {1}", StringHelper.GetBytesReadable(userUploadSize + uploadFile.file.Length), StringHelper.GetBytesReadable(maxTotalSize)) } });
                        }
                    }
                    else
                    {
                        // Non-logged in users are defaulted to 1 day expiration
                        uploadFile.options.ExpirationUnit = ExpirationUnit.Days;
                        uploadFile.options.ExpirationLength = 1;
                    }
                    if (uploadFile.file.Length <= maxUploadSize)
                    {
                        // convert file to bytes
                        long contentLength = uploadFile.file.Length;

                        using (Stream fs = uploadFile.file.OpenReadStream())
                        {
                            ScanResult scanResult = null;

                            // Scan the file to detect a virus
                            if (_config.UploadConfig.ClamConfig.Enabled)
                            {
                                var clamScanner = new ClamScanner(_config);
                                scanResult = await clamScanner.ScanFile(fs);
                            }

                            // Scan the files against an endpoint based on hash
                            if (_config.UploadConfig.HashScanConfig.Enabled && (scanResult == null || scanResult.ResultType == ScanResultType.Clean))
                            {
                                var hashScanner = new HashScanner(_config);
                                scanResult = await hashScanner.ScanFile(fs);
                            }

                            switch (scanResult?.ResultType)
                            {
                                case ScanResultType.Clean:
                                    break;
                                case ScanResultType.VirusDetected:
                                    return Json(new { error = new { message = string.Format("Virus Detected: {0}. As per our <a href=\"{1}\">Terms of Service</a>, Viruses are not permited.", scanResult.RawResult, Url.SubRouteUrl("tos", "TOS.Index")) } });
                                case ScanResultType.ChildPornography:
                                    return Json(new { error = new { message = string.Format("Child Pornography Detected: As per our <a href=\"{0}\">Terms of Service</a>, Child Pornography is not permited.", Url.SubRouteUrl("tos", "TOS.Index")) } });
                                case ScanResultType.Error:
                                    return Json(new { error = new { message = string.Format("Error scanning the file upload.  {0}", scanResult.RawResult) } });
                                case ScanResultType.Unknown:
                                    return Json(new { error = new { message = string.Format("Unknown result while scanning the file upload.  {0}", scanResult.RawResult) } });
                            }

                            // Check content type restrictions (Only for encrypting server side
                            if (!uploadFile.options.Encrypt)
                            {
                                if (_config.UploadConfig.RestrictedContentTypes.Contains(uploadFile.fileType) || _config.UploadConfig.RestrictedExtensions.Contains(uploadFile.fileExt))
                                {
                                    return Json(new { error = new { message = "File Type Not Allowed" } });
                                }
                            }

                            Models.Upload upload = await UploadHelper.SaveFile(_dbContext,
                                _config,
                                fs,
                                uploadFile.fileType,
                                contentLength,
                                !uploadFile.options.Encrypt,
                                uploadFile.options.ExpirationUnit,
                                uploadFile.options.ExpirationLength,
                                uploadFile.fileExt,
                                uploadFile.iv, null,
                                uploadFile.keySize,
                                uploadFile.blockSize);
                            if (upload != null)
                            {
                                if (User.Identity.IsAuthenticated)
                                {
                                    Users.Models.User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                                    if (user != null)
                                    {
                                        upload.UserId = user.UserId;
                                        _dbContext.Entry(upload).State = EntityState.Modified;
                                        _dbContext.SaveChanges();
                                    }
                                }
                                return Json(new
                                {
                                    result = new
                                    {
                                        name = upload.Url,
                                        url = Url.SubRouteUrl("u", "Upload.Download", new { file = upload.Url }),
                                        contentType = upload.ContentType,
                                        contentLength = StringHelper.GetBytesReadable(upload.ContentLength),
                                        contentLengthRaw = upload.ContentLength,
                                        deleteUrl = Url.SubRouteUrl("u", "Upload.DeleteByKey", new { file = upload.Url, key = upload.DeleteKey }),
                                        expirationUnit = uploadFile.options.ExpirationUnit.ToString(),
                                        expirationLength = uploadFile.options.ExpirationLength
                                    }
                                });
                            }
                        }
                        return Json(new { error = new { message = "Unable to upload file" } });
                    }
                    else
                    {
                        return Json(new { error = new { message = "File Too Large.  Max file size is " + StringHelper.GetBytesReadable(maxUploadSize) } });
                    }
                }
                return Json(new { error = new { message = "Uploads are disabled" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception while uploading file: " + ex.GetFullMessage(true) } });
            }
        }
        
        [HttpGet]
        [AllowAnonymous]
        [TrackPageView]
        [TrackDownload]
        [IgnoreAntiforgeryToken]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Download(string file)
        {
            if (_config.UploadConfig.DownloadEnabled)
            {
                ViewBag.Title = "Download " + file;
                string fileName = string.Empty;
                string url = string.Empty;
                string key = string.Empty;
                string iv = string.Empty;
                string contentType = string.Empty;
                long contentLength = 0;
                DateTime dateUploaded = new DateTime();

                var upload = UploadHelper.GetUpload(_dbContext, _cache, file, false);
                if (upload != null)
                {
                    // Check Expiration
                    if (UploadHelper.CheckExpiration(upload))
                    {
                        UploadHelper.DeleteFile(_dbContext, _cache, _config, _logger, file);
                        return new StatusCodeResult(StatusCodes.Status404NotFound);
                    }

                    // Increment the download count for this upload
                    UploadHelper.IncrementDownloadCount(_queue, _cache, _config, file);

                    fileName = upload.FileName;
                    url = upload.Url;
                    key = upload.Key;
                    iv = upload.IV;
                    contentType = upload.ContentType;
                    contentLength = upload.ContentLength;
                    dateUploaded = upload.DateUploaded;
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                // We don't have the key, so we need to decrypt it client side
                if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
                {
                    DownloadViewModel model = new DownloadViewModel();
                    model.CurrentSub = Subdomain;
                    model.FileName = file;
                    model.ContentType = contentType;
                    model.ContentLength = contentLength;
                    model.IV = iv;
                    model.Decrypt = true;

                    return View(model);
                }
                else if (_config.UploadConfig.MaxDownloadFileSize < contentLength)
                {
                    // We want to force them to the dl page due to them being over the max download size for embedded content
                    DownloadViewModel model = new DownloadViewModel();
                    model.CurrentSub = Subdomain;
                    model.FileName = file;
                    model.ContentType = contentType;
                    model.ContentLength = contentLength;
                    model.Decrypt = false;

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
                        return new StatusCodeResult(StatusCodes.Status304NotModified);
                    }
                    else
                    {
                        var storageService = StorageServiceFactory.GetStorageService(_config.UploadConfig.StorageConfig);
                        var fileStream = storageService.GetFile(fileName);
                        long startByte = 0;
                        long endByte = contentLength - 1;
                        long length = contentLength;
                        if (fileStream != null)
                        {
                            Response.RegisterForDispose(fileStream);

                            #region Range Calculation
                            // Are they downloading it by range?
                            bool byRange = !string.IsNullOrEmpty(Request.Headers["Range"]); // We do not support ranges

                            // check to see if we need to pass a specified range
                            if (byRange)
                            {
                                long anotherStart = startByte;
                                long anotherEnd = endByte;
                                string[] arr_split = Request.Headers["Range"].ToString().Split(new char[] { '=' });
                                string range = arr_split[1];

                                // Make sure the client hasn't sent us a multibyte range 
                                if (range.IndexOf(",") > -1)
                                {
                                    // (?) Shoud this be issued here, or should the first 
                                    // range be used? Or should the header be ignored and 
                                    // we output the whole content? 
                                    Response.Headers.Add("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                                    return new StatusCodeResult(StatusCodes.Status416RequestedRangeNotSatisfiable);
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

                                    Response.Headers.Add("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                                    return new StatusCodeResult(StatusCodes.Status416RequestedRangeNotSatisfiable);
                                }
                                startByte = anotherStart;
                                endByte = anotherEnd;

                                length = endByte - startByte + 1; // Calculate new content length 

                                // Ranges are response of 206
                                Response.StatusCode = 206;
                            }
                            #endregion

                            // Set Last Modified
                            Response.GetTypedHeaders().LastModified = dateUploaded;

                            // We accept ranges
                            Response.Headers.Add("Accept-Ranges", "0-" + contentLength);

                            // Notify the client the byte range we'll be outputting 
                            Response.Headers.Add("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                            // Notify the client the content length we'll be outputting 
                            Response.Headers.Add("Content-Length", length.ToString());

                            // Set the content type of this response
                            Response.Headers.Add("Content-Type", contentType);

                            // Create content disposition
                            var cd = new System.Net.Mime.ContentDisposition
                            {
                                FileName = url,
                                Inline = true
                            };

                            Response.Headers.Add("Content-Disposition", cd.ToString());

                            // Reset file stream to starting position (or start of range)
                            fileStream.Seek(startByte, SeekOrigin.Begin);

                            return DownloadData(url, fileStream, contentType, (int)length, key, iv);
                        }
                    }
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
            }
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [HttpPost]
        [AllowAnonymous]
        [TrackDownload]
        public IActionResult DownloadData(string file, bool decrypt)
        {
            if (_config.UploadConfig.DownloadEnabled)
            {
                Models.Upload upload =  UploadHelper.GetUpload(_dbContext, _cache, file, false);
                if (upload != null)
                {
                    // Check Expiration
                    if (UploadHelper.CheckExpiration(upload))
                    {
                        UploadHelper.DeleteFile(_dbContext, _cache, _config, _logger, file);
                        return Json(new { error = new { message = "File Does Not Exist" } });
                    }

                    var storageService = StorageServiceFactory.GetStorageService(_config.UploadConfig.StorageConfig);
                    var fileStream = storageService.GetFile(upload.FileName);
                    if (fileStream != null)
                    {
                        Response.RegisterForDispose(fileStream);

                        // Notify the client the content length we'll be outputting 
                        Response.Headers.Add("Content-Length", upload.ContentLength.ToString());

                        // Create content disposition
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            FileName = upload.Url,
                            Inline = true
                        };

                        // Set the content type of this response
                        Response.Headers.Add("Content-Type", upload.ContentType);

                        Response.Headers.Add("Content-Disposition", cd.ToString());

                        return DownloadData(upload.Url, fileStream, upload.ContentType, (int)upload.ContentLength, upload.Key, upload.IV);
                    }
                }
                return Json(new { error = new { message = "File Does Not Exist" } });
            }
            return Json(new { error = new { message = "Downloads are disabled" } });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DeleteByKey(string file, string key)
        {
            ViewBag.Title = "File Delete | " + file ;
            Models.Upload upload = UploadHelper.GetUpload(_dbContext, _cache, file, false);
            if (upload != null)
            {
                DeleteViewModel model = new DeleteViewModel();
                model.File = file;
                if (!string.IsNullOrEmpty(upload.DeleteKey) && upload.DeleteKey == key)
                {
                    UploadHelper.DeleteFile(_dbContext, _cache, _config, _logger, file);
                    model.Deleted = true;
                }
                else
                {
                    model.Deleted = false;
                }
                return View(model);
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        public IActionResult GenerateDeleteKey(string file)
        {
            Models.Upload upload = UploadHelper.GetUpload(_dbContext, _cache, file);
            if (upload != null)
            {
                if (upload.User?.Username == User.Identity.Name ||
                    User.IsInRole("Admin"))
                {
                    var delKey = UploadHelper.GenerateDeleteKey(_dbContext, _cache, _config, file);
                    return Json(new { result = new { url = Url.SubRouteUrl("u", "Upload.DeleteByKey", new { file = file, key = delKey }) } });
                }
                return Json(new { error = new { message = "You do not have permission to delete this Upload" } });
            }
            return Json(new { error = new { message = "Invalid URL" } });
        }

        [HttpPost]
        [HttpOptions]
        public IActionResult Delete(string id)
        {
            Models.Upload foundUpload = UploadHelper.GetUpload(_dbContext, _cache, id);
            if (foundUpload != null)
            {
                if (foundUpload.User?.Username == User.Identity.Name ||
                    User.IsInRole("Admin"))
                {
                    UploadHelper.DeleteFile(_dbContext, _cache, _config, _logger, id);
                    return Json(new { result = true });
                }
                return Json(new { error = new { message = "You do not have permission to delete this Upload" } });
            }
            return Json(new { error = new { message = "This Upload does not exist" } });
        }

        private IActionResult DownloadData(string url, Stream fileStream, string contentType, int length, string key, string iv)
        {
            try
            {
                // If the IV is set, and Key is set, then decrypt it while sending
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
                    
                    var keyArray = new PooledArray(keyBytes);
                    var ivArray = new PooledArray(ivBytes);

                    Response.RegisterForDispose(keyArray);
                    Response.RegisterForDispose(ivArray);

                    var aesStream = new AesCounterStream(fileStream, false, keyArray, ivArray);
                    Response.RegisterForDispose(aesStream);

                    //return File(aesStream, contentType, true);
                    return new BufferedFileStreamResult(contentType, async (response) => await ResponseHelper.StreamToOutput(response, aesStream, length, _config.UploadConfig.ChunkSize), false);
                }
                else // Otherwise just send it
                {
                    // Send the file
                    return new BufferedFileStreamResult(contentType, async (response) => await ResponseHelper.StreamToOutput(response, fileStream, length, _config.UploadConfig.ChunkSize), false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in Download: {url}", new { url });
            }

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
