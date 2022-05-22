using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeDetective;
using MimeDetective.Extensions;
using Teknik.Areas.API.V1.Models;
using Teknik.Areas.Upload;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.ContentScanningService;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Utilities;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.API.V1.Controllers
{
    [Authorize(Policy = "WriteAPI")]
    public class UploadAPIv1Controller : APIv1Controller
    {
        public UploadAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [HttpPost]
        [TrackPageView]
        public async Task<IActionResult> Upload(UploadAPIv1Model model)
        {
            try
            {
                if (_config.UploadConfig.UploadEnabled)
                {
                    if (model.file != null)
                    {
                        long maxUploadSize = _config.UploadConfig.MaxUploadFileSize;
                        long maxTotalSize = _config.UploadConfig.MaxUploadFileSize;
                        if (User.Identity.IsAuthenticated)
                        {
                            // Check account total limits
                            var user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                            if (user.UploadSettings.MaxUploadStorage != null)
                                maxTotalSize = user.UploadSettings.MaxUploadStorage.Value;
                            if (user.UploadSettings.MaxUploadFileSize != null)
                                maxUploadSize = user.UploadSettings.MaxUploadFileSize.Value;

                            var userUploadSize = user.Uploads.Sum(u => u.ContentLength);
                            if (userUploadSize + model.file.Length > maxTotalSize)
                            {
                                return Json(new { error = new { message = string.Format("Account storage limit exceeded: {0} / {1}", StringHelper.GetBytesReadable(userUploadSize + model.file.Length), StringHelper.GetBytesReadable(maxTotalSize)) } });
                            }
                        }
                        else
                        {
                            // Non-logged in users are defaulted to 1 day expiration
                            model.expirationUnit = ExpirationUnit.Days;
                            model.expirationLength = 1;
                        }
                        if (model.file.Length <= maxUploadSize)
                        {
                            // convert file to bytes
                            string fileExt = FileHelper.GetFileExtension(model.file.FileName);
                            long contentLength = model.file.Length;

                            using (var fs = model.file.OpenReadStream())
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
                                        return Json(new { error = new { message = string.Format("Child Pornography Detected: As per our <a href=\"{0}\">Terms of Service</a>, Child Pornography is strictly not permited.", Url.SubRouteUrl("tos", "TOS.Index")) } });
                                    case ScanResultType.Error:
                                        return Json(new { error = new { message = string.Format("Error scanning the file upload.  {0}", scanResult.RawResult) } });
                                    case ScanResultType.Unknown:
                                        return Json(new { error = new { message = string.Format("Unknown result while scanning the file upload.  {0}", scanResult.RawResult) } });
                                }

                                // Need to grab the contentType if it's empty
                                if (string.IsNullOrEmpty(model.contentType))
                                {
                                    model.contentType = model.file.ContentType;

                                    if (string.IsNullOrEmpty(model.contentType))
                                    {
                                        fs.Seek(0, SeekOrigin.Begin);
                                        FileType fileType = fs.GetFileType();
                                        if (fileType != null)
                                            model.contentType = fileType.Mime;
                                        if (string.IsNullOrEmpty(model.contentType))
                                        {
                                            model.contentType = "application/octet-stream";
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
                                Upload.Models.Upload upload = await UploadHelper.SaveFile(_dbContext, _config, fs, model.contentType, contentLength, model.encrypt, model.expirationUnit, model.expirationLength, fileExt, model.iv, model.key, model.keySize, model.blockSize);

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
                                        maxDownloads = upload.MaxDownloads,
                                        expirationDate = upload.ExpireDate,
                                        deletionKey = upload.DeleteKey

                                    };
                                    return Json(new { result = returnData });
                                }
                            }
                            return Json(new { error = new { message = "Unable to save file" } });

                        }
                        else
                        {
                            return Json(new { error = new { message = "File Too Large.  Max file size is " + StringHelper.GetBytesReadable(maxUploadSize) } });
                        }
                    }
                    return Json(new { error = new { message = "Invalid Upload Request" } });
                }
                return Json(new { error = new { message = "Uploads are Disabled" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }
    }
}