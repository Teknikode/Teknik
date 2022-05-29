﻿using System;
using System.Linq;
using System.Text;
using Teknik.Areas.Paste.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Attributes;
using Teknik.Utilities.Cryptography;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Teknik.Logging;
using System.IO;
using System.Threading.Tasks;
using Teknik.Utilities.Routing;
using Teknik.StorageService;

namespace Teknik.Areas.Paste.Controllers
{
    [Authorize]
    [Area("Paste")]
    public class PasteController : DefaultController
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly ObjectCache _cache;

        public PasteController(ILogger<Logger> logger, Config config, TeknikEntities dbContext, IBackgroundTaskQueue queue, ObjectCache cache) : base(logger, config, dbContext)
        {
            _queue = queue;
            _cache = cache;
        }

        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index()
        {
            ViewBag.Title = "Pastebin";
            ViewBag.Description = "Paste your code or text easily and securely.  Set an expiration, set a password, or leave it open for the world to see.";
            PasteCreateViewModel model = new PasteCreateViewModel();
            return View(model);
        }
        
        [AllowAnonymous]
        [TrackPageView]
        public async Task<IActionResult> ViewPaste(string type, string url, string password)
        {
            Models.Paste paste = PasteHelper.GetPaste(_dbContext, _cache, url);
            if (paste != null)
            {
                ViewBag.Title = (string.IsNullOrEmpty(paste.Title)) ? "Untitled Paste" : paste.Title + " | Pastebin";
                ViewBag.Description = "Paste your code or text easily and securely.  Set an expiration, set a password, or leave it open for the world to see.";

                string fileName = paste.FileName;
                string key = paste.Key;
                string iv = paste.IV;
                int blockSize = paste.BlockSize;
                int keySize = paste.KeySize;
                string hashedPass = paste.HashedPassword;

                // Check Expiration
                if (PasteHelper.CheckExpiration(paste))
                {
                    PasteHelper.DeleteFile(_dbContext, _cache, _config, _logger, url);
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                // Increment View Count
                PasteHelper.IncrementViewCount(_queue, _cache, _config, url);

                PasteViewModel model = new PasteViewModel();
                model.Url = url;
                model.Title = paste.Title;
                model.Syntax = paste.Syntax;
                model.DatePosted = paste.DatePosted;
                model.Username = paste.User?.Username;

                if (User.Identity.IsAuthenticated && type.ToLower() == "full")
                {
                    Users.Models.User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        model.Vaults = user.Vaults.ToList();
                    }
                }

                PooledArray ivArray;
                if (!string.IsNullOrEmpty(iv))
                {
                    var ivBytes = Encoding.Unicode.GetBytes(iv);
                    ivArray = new PooledArray(ivBytes);
                }
                else
                    ivArray = new PooledArray(blockSize);
                Response.RegisterForDispose(ivArray);

                PooledArray keyArray;
                if (!string.IsNullOrEmpty(key))
                {
                    var keyBytes = AesCounterManaged.CreateKey(key, ivArray.Array, keySize);
                    keyArray = new PooledArray(keyBytes);
                }
                else
                    keyArray = new PooledArray(keySize);
                Response.RegisterForDispose(keyArray);

                // The paste has a password set
                if (!string.IsNullOrEmpty(hashedPass))
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        // Try to get the password from the session
                        password = GetCachedPassword(url);
                    }
                    string hash = string.Empty;
                    if (!string.IsNullOrEmpty(password))
                    {
                        hash = Crypto.HashPassword(key, password);
                        AesCounterManaged.CreateKey(password, ivArray.Array, keySize).CopyTo(keyArray.Array, 0);
                    }
                    if (string.IsNullOrEmpty(password) || hash != hashedPass)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
                        passModel.ActionUrl = Url.SubRouteUrl("p", "Paste.View", new { type = type, url = url });
                        passModel.Url = url;
                        passModel.Type = type;

                        if (!string.IsNullOrEmpty(password) && hash != hashedPass)
                        {
                            passModel.Error = true;
                            passModel.ErrorMessage = "Invalid Password";
                        }

                        // Redirect them to the password request page
                        return View("~/Areas/Paste/Views/Paste/PasswordNeeded.cshtml", passModel);
                    }
                }

                // Save the password to the cache
                CachePassword(url, password);

                // Read in the file
                if (string.IsNullOrEmpty(fileName))
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                var storageService = StorageServiceFactory.GetStorageService(_config.PasteConfig.StorageConfig);
                using var fileStream = storageService.GetFile(fileName);
                if (fileStream == null)
                    return new StatusCodeResult(StatusCodes.Status404NotFound);

                int contentSize = (int)fileStream.Length;

                // Only load the model content if we aren't downloading it.
                if (type.ToLower() != "download")
                {
                    using (AesCounterStream cs = new AesCounterStream(fileStream, false, keyArray, ivArray))
                    using (StreamReader sr = new StreamReader(cs, Encoding.Unicode))
                    {
                        model.Content = await sr.ReadToEndAsync();
                    }
                }

                switch (type.ToLower())
                {
                    case "full":
                        return View("~/Areas/Paste/Views/Paste/Full.cshtml", model);
                    case "simple":
                        return View("~/Areas/Paste/Views/Paste/Simple.cshtml", model);
                    case "raw":
                        return Content(model.Content, "text/plain");
                    case "download":
                        //Create File
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            FileName = url + ".txt",
                            Inline = true
                        };

                        Response.Headers.Add("Content-Disposition", cd.ToString());

                        return new BufferedFileStreamResult("application/octet-stream", async (response) => await ResponseHelper.StreamToOutput(response, new AesCounterStream(fileStream, false, keyArray, ivArray), contentSize, _config.PasteConfig.ChunkSize), false);
                    default:
                        return View("~/Areas/Paste/Views/Paste/Full.cshtml", model);
                }
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Paste([Bind("Content, Title, Syntax, ExpireLength, ExpireUnit, Password")]PasteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_config.PasteConfig.Enabled)
                {
                    try
                    {
                        Models.Paste paste = await PasteHelper.CreatePaste(_config, _dbContext, model.Content, model.Title, model.Syntax, model.ExpireUnit, model.ExpireLength ?? 1, model.Password);

                        if (model.ExpireUnit == ExpirationUnit.Views)
                        {
                            paste.Views = -1;
                        }

                        if (User.Identity.IsAuthenticated)
                        {
                            Users.Models.User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                            if (user != null)
                            {
                                paste.UserId = user.UserId;
                            }
                        }

                        _dbContext.Pastes.Add(paste);
                        _dbContext.SaveChanges();

                        // Cache the password
                        CachePassword(paste.Url, model.Password);

                        return Redirect(Url.SubRouteUrl("p", "Paste.View", new { type = "Full", url = paste.Url }));
                    }
                    catch (Exception ex)
                    {
                        return Redirect(Url.SubRouteUrl("error", "Error.500", new { exception = ex }));
                    }
                }
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            return View("~/Areas/Paste/Views/Paste/Index.cshtml", model);
        }

        [TrackPageView]
        public async Task<IActionResult> Edit(string url, string password)
        {
            Models.Paste paste = PasteHelper.GetPaste(_dbContext, _cache, url);
            if (paste != null)
            {
                if (paste.User?.Username != User.Identity.Name)
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);

                ViewBag.Title = "Edit Paste";
                ViewBag.Description = "Edit your paste's content.";

                // Check Expiration
                if (PasteHelper.CheckExpiration(paste))
                {
                    PasteHelper.DeleteFile(_dbContext, _cache, _config, _logger, url);
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                PasteViewModel model = new PasteViewModel();
                model.Url = url;
                model.Title = paste.Title;
                model.Syntax = paste.Syntax;
                model.DatePosted = paste.DatePosted;
                model.Username = paste.User?.Username;

                PooledArray ivArray;
                if (!string.IsNullOrEmpty(paste.IV))
                {
                    var ivBytes = Encoding.Unicode.GetBytes(paste.IV);
                    ivArray = new PooledArray(ivBytes);
                }
                else
                    ivArray = new PooledArray(paste.BlockSize);
                Response.RegisterForDispose(ivArray);

                PooledArray keyArray;
                if (!string.IsNullOrEmpty(paste.Key))
                {
                    var keyBytes = AesCounterManaged.CreateKey(paste.Key, ivArray.Array, paste.KeySize);
                    keyArray = new PooledArray(keyBytes);
                }
                else
                    keyArray = new PooledArray(paste.KeySize);
                Response.RegisterForDispose(keyArray);

                // The paste has a password set
                if (!string.IsNullOrEmpty(paste.HashedPassword))
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        // Try to get the password from the session
                        password = GetCachedPassword(url);
                    }
                    string hash = string.Empty;
                    if (!string.IsNullOrEmpty(password))
                    {
                        hash = Crypto.HashPassword(paste.Key, password);
                        AesCounterManaged.CreateKey(password, ivArray.Array, paste.KeySize).CopyTo(keyArray.Array, 0);
                    }
                    if (string.IsNullOrEmpty(password) || hash != paste.HashedPassword)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
                        passModel.ActionUrl = Url.SubRouteUrl("p", "Paste.Edit");
                        passModel.Url = url;

                        if (!string.IsNullOrEmpty(password) && hash != paste.HashedPassword)
                        {
                            passModel.Error = true;
                            passModel.ErrorMessage = "Invalid Password";
                        }

                        // Redirect them to the password request page
                        return View("~/Areas/Paste/Views/Paste/PasswordNeeded.cshtml", passModel);
                    }
                }

                // Cache the password
                CachePassword(url, password);

                // Read in the file
                if (string.IsNullOrEmpty(paste.FileName))
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                var storageService = StorageServiceFactory.GetStorageService(_config.PasteConfig.StorageConfig);
                using var fileStream = storageService.GetFile(paste.FileName);
                if (fileStream == null)
                    return new StatusCodeResult(StatusCodes.Status404NotFound);

                using (AesCounterStream cs = new AesCounterStream(fileStream, false, keyArray, ivArray))
                using (StreamReader sr = new StreamReader(cs, Encoding.Unicode))
                {
                    model.Content = await sr.ReadToEndAsync();
                }

                return View("~/Areas/Paste/Views/Paste/Edit.cshtml", model);
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> EditSubmit([Bind("Content, Title, Syntax, Url")]PasteEditViewModel model)
        {
            if (_config.PasteConfig.Enabled)
            {
                try
                {
                    Models.Paste paste = PasteHelper.GetPaste(_dbContext, _cache, model.Url);
                    if (paste != null)
                    {
                        if (paste.User?.Username != User.Identity.Name)
                            return new StatusCodeResult(StatusCodes.Status403Forbidden);

                        string password = null;
                        // The paste has a password set
                        if (!string.IsNullOrEmpty(paste.HashedPassword))
                        {
                            // Try to get the password from the session
                            password = GetCachedPassword(model.Url);
                            string hash = string.Empty;
                            if (!string.IsNullOrEmpty(password))
                            {
                                hash = Crypto.HashPassword(paste.Key, password);
                            }
                            if (string.IsNullOrEmpty(password) || hash != paste.HashedPassword)
                            {
                                PasswordViewModel passModel = new PasswordViewModel();
                                passModel.ActionUrl = Url.SubRouteUrl("p", "Paste.Edit");
                                passModel.Url = model.Url;

                                if (!string.IsNullOrEmpty(password) && hash != paste.HashedPassword)
                                {
                                    passModel.Error = true;
                                    passModel.ErrorMessage = "Invalid Password";
                                }

                                // Redirect them to the password request page
                                return View("~/Areas/Paste/Views/Paste/PasswordNeeded.cshtml", passModel);
                            }
                        }

                        // get the old file
                        var storageService = StorageServiceFactory.GetStorageService(_config.PasteConfig.StorageConfig);
                        var oldFile = paste.FileName;

                        // Generate a unique file name that does not currently exist
                        string fileName = storageService.GetUniqueFileName();

                        string key = Crypto.GenerateKey(_config.PasteConfig.KeySize);
                        string iv = Crypto.GenerateIV(_config.PasteConfig.BlockSize);

                        await PasteHelper.EncryptContents(storageService, model.Content, fileName, password, key, iv, _config.PasteConfig.KeySize, _config.PasteConfig.ChunkSize);

                        paste.Key = key;
                        paste.KeySize = _config.PasteConfig.KeySize;
                        paste.IV = iv;
                        paste.BlockSize = _config.PasteConfig.BlockSize;

                        if (!string.IsNullOrEmpty(password))
                            paste.HashedPassword = Crypto.HashPassword(paste.Key, password);
                        paste.FileName = fileName;
                        paste.Title = model.Title;
                        paste.Syntax = model.Syntax;
                        paste.DateEdited = DateTime.Now;

                        PasteHelper.ModifyPaste(_dbContext, _cache, paste);

                        // Delete the old file
                        storageService.DeleteFile(oldFile);

                        return Redirect(Url.SubRouteUrl("p", "Paste.View", new { type = "Full", url = paste.Url }));
                    }
                }
                catch (Exception ex)
                {
                    return Redirect(Url.SubRouteUrl("error", "Error.500", new { exception = ex }));
                }
            }
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [HttpPost]
        [HttpOptions]
        public IActionResult Delete(string id)
        {
            Models.Paste foundPaste = PasteHelper.GetPaste(_dbContext, _cache, id);
            if (foundPaste != null)
            {
                if (foundPaste.User?.Username == User.Identity.Name ||
                    User.IsInRole("Admin"))
                {
                    PasteHelper.DeleteFile(_dbContext, _cache, _config, _logger, id);

                    return Json(new { result = true, redirect = Url.SubRouteUrl("p", "Paste.Index") });
                }
                return Json(new { error = new { message = "You do not have permission to delete this Paste" } });
            }
            return Json(new { error = new { message = "This Paste does not exist" } });
        }

        private void CachePassword(string url, string password)
        {
            if (HttpContext != null && HttpContext.Session != null)
            {
                HttpContext.Session?.Set("PastePassword_" + url, password);
            }
        }

        private string GetCachedPassword(string url)
        {
            if (HttpContext != null && HttpContext.Session != null)
            {
                return HttpContext.Session.Get<string>("PastePassword_" + url);
            }
            return null;
        }

        private void ClearCachedPassword(string url)
        {
            if (HttpContext != null)
            {
                HttpContext.Session.Remove("PastePassword_" + url);
            }
        }
    }
}