using nClam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teknik.Areas.Upload.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Models;
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

namespace Teknik.Areas.Upload.Controllers
{
    [Authorize]
    [Area("Upload")]
    public class UploadController : DefaultController
    {
        public UploadController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Upload Files";
            UploadViewModel model = new UploadViewModel();
            model.CurrentSub = Subdomain;
            model.Encrypt = false;
            model.ExpirationLength = 1;
            model.ExpirationUnit = ExpirationUnit.Days;
            model.MaxUploadSize = _config.UploadConfig.MaxUploadSize;
            if (User.Identity.IsAuthenticated)
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    model.Encrypt = user.UploadSettings.Encrypt;
                    model.ExpirationLength = user.UploadSettings.ExpirationLength;
                    model.ExpirationUnit = user.UploadSettings.ExpirationUnit;
                    model.Vaults = user.Vaults.ToList();

                    model.MaxUploadSize = _config.UploadConfig.MaxUploadSizeBasic;
                    IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                    if (userInfo.AccountType == AccountType.Premium)
                    {
                        model.MaxUploadSize = _config.UploadConfig.MaxUploadSizePremium;
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
                    long maxUploadSize = _config.UploadConfig.MaxUploadSize;
                    if (User.Identity.IsAuthenticated)
                    {
                        maxUploadSize = _config.UploadConfig.MaxUploadSizeBasic;
                        IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                        if (userInfo.AccountType == AccountType.Premium)
                        {
                            maxUploadSize = _config.UploadConfig.MaxUploadSizePremium;
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

                        // Scan the file to detect a virus
                        if (_config.UploadConfig.VirusScanEnable)
                        {
                            using (Stream fs = uploadFile.file.OpenReadStream())
                            {
                                ClamClient clam = new ClamClient(_config.UploadConfig.ClamServer, _config.UploadConfig.ClamPort);
                                clam.MaxStreamSize = maxUploadSize;
                                ClamScanResult scanResult = await clam.SendAndScanFileAsync(fs);

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
                        }

                        // Check content type restrictions (Only for encrypting server side
                        if (!uploadFile.options.Encrypt)
                        {
                            if (_config.UploadConfig.RestrictedContentTypes.Contains(uploadFile.fileType) || _config.UploadConfig.RestrictedExtensions.Contains(uploadFile.fileExt))
                            {
                                return Json(new { error = new { message = "File Type Not Allowed" } });
                            }
                        }

                        using (Stream fs = uploadFile.file.OpenReadStream())
                        {
                            Models.Upload upload = UploadHelper.SaveFile(_dbContext, 
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
                                return Json(new { result = new
                                {
                                    name = upload.Url,
                                    url = Url.SubRouteUrl("u", "Upload.Download", new { file = upload.Url }),
                                    contentType = upload.ContentType,
                                    contentLength = StringHelper.GetBytesReadable(upload.ContentLength),
                                    deleteUrl = Url.SubRouteUrl("u", "Upload.DeleteByKey", new { file = upload.Url, key = upload.DeleteKey }),
                                    expirationUnit = uploadFile.options.ExpirationUnit.ToString(),
                                    expirationLength = uploadFile.options.ExpirationLength
                                } });
                            }
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
        
        [HttpGet]
        [AllowAnonymous]
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
                bool premiumAccount = false;
                DateTime dateUploaded = new DateTime();

                Models.Upload upload = _dbContext.Uploads.Where(up => up.Url == file).FirstOrDefault();
                if (upload != null)
                {
                    // Check Expiration
                    if (UploadHelper.CheckExpiration(upload))
                    {
                        _dbContext.Uploads.Remove(upload);
                        _dbContext.SaveChanges();
                        return new StatusCodeResult(StatusCodes.Status404NotFound);
                    }

                    upload.Downloads += 1;
                    _dbContext.Entry(upload).State = EntityState.Modified;
                    _dbContext.SaveChanges();

                    fileName = upload.FileName;
                    url = upload.Url;
                    key = upload.Key;
                    iv = upload.IV;
                    contentType = upload.ContentType;
                    contentLength = upload.ContentLength;
                    dateUploaded = upload.DateUploaded;
                    if (User.Identity.IsAuthenticated)
                    {
                        IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                        premiumAccount = userInfo.AccountType == AccountType.Premium;
                    }
                    if (!premiumAccount && upload.User != null)
                    {
                        IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, upload.User.Username);
                        premiumAccount = userInfo.AccountType == AccountType.Premium;
                    }
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
                else if (!premiumAccount && _config.UploadConfig.MaxDownloadSize < contentLength)
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
                        string subDir = fileName[0].ToString();
                        string filePath = Path.Combine(_config.UploadConfig.UploadDirectory, subDir, fileName);
                        long startByte = 0;
                        long endByte = contentLength - 1;
                        long length = contentLength;
                        if (System.IO.File.Exists(filePath))
                        {
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

                                    return new BufferedFileStreamResult(contentType, async (response) => await ResponseHelper.StreamToOutput(response, true, new AesCounterStream(fs, false, keyBytes, ivBytes), (int)length, _config.UploadConfig.ChunkSize), false);
                                }
                                else // Otherwise just send it
                                {
                                    // Send the file
                                    return new BufferedFileStreamResult(contentType, async (response) => await ResponseHelper.StreamToOutput(response, true, fs, (int)length, _config.UploadConfig.ChunkSize), false);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error in Download: {url}", new { url });
                            }
                        }
                    }
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
            }
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult DownloadData(string file, bool decrypt)
        {
            if (_config.UploadConfig.DownloadEnabled)
            {
                Models.Upload upload = _dbContext.Uploads.Where(up => up.Url == file).FirstOrDefault();
                if (upload != null)
                {
                    // Check Expiration
                    if (UploadHelper.CheckExpiration(upload))
                    {
                        _dbContext.Uploads.Remove(upload);
                        _dbContext.SaveChanges();
                        return Json(new { error = new { message = "File Does Not Exist" } });
                    }

                    string subDir = upload.FileName[0].ToString();
                    string filePath = Path.Combine(_config.UploadConfig.UploadDirectory, subDir, upload.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
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

                        // Read in the file
                        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                        // If the IV is set, and Key is set, then decrypt it while sending
                        if (decrypt && !string.IsNullOrEmpty(upload.Key) && !string.IsNullOrEmpty(upload.IV))
                        {
                            byte[] keyBytes = Encoding.UTF8.GetBytes(upload.Key);
                            byte[] ivBytes = Encoding.UTF8.GetBytes(upload.IV);

                            return new BufferedFileStreamResult(upload.ContentType, (response) => ResponseHelper.StreamToOutput(response, true, new AesCounterStream(fs, false, keyBytes, ivBytes), (int)upload.ContentLength, _config.UploadConfig.ChunkSize), false);
                        }
                        else // Otherwise just send it
                        {
                            // Send the file
                            return new BufferedFileStreamResult(upload.ContentType, (response) => ResponseHelper.StreamToOutput(response, true, fs, (int)upload.ContentLength, _config.UploadConfig.ChunkSize), false);
                        }
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
            Models.Upload upload = _dbContext.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                DeleteViewModel model = new DeleteViewModel();
                model.File = file;
                if (!string.IsNullOrEmpty(upload.DeleteKey) && upload.DeleteKey == key)
                {
                    string filePath = upload.FileName;
                    // Delete from the DB
                    _dbContext.Uploads.Remove(upload);
                    _dbContext.SaveChanges();

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
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        public IActionResult GenerateDeleteKey(string file)
        {
            Models.Upload upload = _dbContext.Uploads.Where(up => up.Url == file).FirstOrDefault();
            if (upload != null)
            {
                if (upload.User.Username == User.Identity.Name)
                {
                    string delKey = StringHelper.RandomString(_config.UploadConfig.DeleteKeyLength);
                    upload.DeleteKey = delKey;
                    _dbContext.Entry(upload).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                    return Json(new { result = new { url = Url.SubRouteUrl("u", "Upload.DeleteByKey", new { file = file, key = delKey }) } });
                }
                return Json(new { error = new { message = "You do not own this upload" } });
            }
            return Json(new { error = new { message = "Invalid URL" } });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            Models.Upload foundUpload = _dbContext.Uploads.Where(u => u.Url == id).FirstOrDefault();
            if (foundUpload != null)
            {
                if (foundUpload.User.Username == User.Identity.Name)
                {
                    string filePath = foundUpload.FileName;
                    // Delete from the DB
                    _dbContext.Uploads.Remove(foundUpload);
                    _dbContext.SaveChanges();

                    // Delete the File
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    return Json(new { result = true });
                }
                return Json(new { error = new { message = "You do not have permission to edit this Paste" } });
            }
            return Json(new { error = new { message = "This Paste does not exist" } });
        }
    }
}
