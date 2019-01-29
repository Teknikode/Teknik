using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teknik.Areas.Paste.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Attributes;
using Teknik.Utilities.Cryptography;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Teknik.Logging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;

namespace Teknik.Areas.Paste.Controllers
{
    [Authorize]
    [Area("Paste")]
    public class PasteController : DefaultController
    {
        public PasteController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Pastebin";
            ViewBag.Description = "Paste your code or text easily and securely.  Set an expiration, set a password, or leave it open for the world to see.";
            PasteCreateViewModel model = new PasteCreateViewModel();
            return View(model);
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> ViewPaste(string type, string url, string password)
        {
            Models.Paste paste = _dbContext.Pastes.Where(p => p.Url == url).FirstOrDefault();
            if (paste != null)
            {
                ViewBag.Title = paste.Title + " | Pastebin";
                ViewBag.Description = "Paste your code or text easily and securely.  Set an expiration, set a password, or leave it open for the world to see.";
                // Increment Views
                paste.Views += 1;
                _dbContext.Entry(paste).State = EntityState.Modified;
                _dbContext.SaveChanges();

                // Check Expiration
                if (PasteHelper.CheckExpiration(paste))
                {
                    _dbContext.Pastes.Remove(paste);
                    _dbContext.SaveChanges();
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

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

                byte[] ivBytes = (string.IsNullOrEmpty(paste.IV)) ? new byte[paste.BlockSize] : Encoding.Unicode.GetBytes(paste.IV);
                byte[] keyBytes = (string.IsNullOrEmpty(paste.Key)) ? new byte[paste.KeySize] : AesCounterManaged.CreateKey(paste.Key, ivBytes, paste.KeySize);

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
                        hash = PasteHelper.HashPassword(paste.Key, password);
                        keyBytes = AesCounterManaged.CreateKey(password, ivBytes, paste.KeySize);
                    }
                    if (string.IsNullOrEmpty(password) || hash != paste.HashedPassword)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
                        passModel.ActionUrl = Url.SubRouteUrl("p", "Paste.View");
                        passModel.Url = url;
                        passModel.Type = type;

                        if (!string.IsNullOrEmpty(password) && hash != paste.HashedPassword)
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
                string subDir = paste.FileName[0].ToString();
                string filePath = Path.Combine(_config.PasteConfig.PasteDirectory, subDir, paste.FileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (AesCounterStream cs = new AesCounterStream(fs, false, keyBytes, ivBytes))
                using (StreamReader sr = new StreamReader(cs, Encoding.Unicode))
                {
                    model.Content = await sr.ReadToEndAsync();
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

                        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        return new BufferedFileStreamResult("application/octet-stream", async (response) => await ResponseHelper.StreamToOutput(response, true, new AesCounterStream(fs, false, keyBytes, ivBytes), (int)fs.Length, _config.PasteConfig.ChunkSize), false);
                    default:
                        return View("~/Areas/Paste/Views/Paste/Full.cshtml", model);
                }
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [AllowAnonymous]
        [DisableRequestSizeLimit]
        public IActionResult Paste([Bind("Content, Title, Syntax, ExpireLength, ExpireUnit, Password")]PasteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_config.PasteConfig.Enabled)
                {
                    try
                    {
                        Models.Paste paste = PasteHelper.CreatePaste(_config, _dbContext, model.Content, model.Title, model.Syntax, model.ExpireUnit, model.ExpireLength ?? 1, model.Password);

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
        
        public async Task<IActionResult> Edit(string url, string password)
        {
            Models.Paste paste = _dbContext.Pastes.Where(p => p.Url == url).FirstOrDefault();
            if (paste != null)
            {
                if (paste.User?.Username != User.Identity.Name)
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);

                ViewBag.Title = "Edit Paste";
                ViewBag.Description = "Edit your paste's content.";

                // Check Expiration
                if (PasteHelper.CheckExpiration(paste))
                {
                    _dbContext.Pastes.Remove(paste);
                    _dbContext.SaveChanges();
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                PasteViewModel model = new PasteViewModel();
                model.Url = url;
                model.Title = paste.Title;
                model.Syntax = paste.Syntax;
                model.DatePosted = paste.DatePosted;
                model.Username = paste.User?.Username;

                byte[] ivBytes = Encoding.Unicode.GetBytes(paste.IV);
                byte[] keyBytes = AesCounterManaged.CreateKey(paste.Key, ivBytes, paste.KeySize);

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
                        hash = PasteHelper.HashPassword(paste.Key, password);
                        keyBytes = AesCounterManaged.CreateKey(password, ivBytes, paste.KeySize);
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
                string subDir = paste.FileName[0].ToString();
                string filePath = Path.Combine(_config.PasteConfig.PasteDirectory, subDir, paste.FileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (AesCounterStream cs = new AesCounterStream(fs, false, keyBytes, ivBytes))
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
        public IActionResult EditSubmit([Bind("Content, Title, Syntax, Url")]PasteEditViewModel model)
        {
            if (_config.PasteConfig.Enabled)
            {
                try
                {
                    Models.Paste paste = _dbContext.Pastes.Where(p => p.Url == model.Url).FirstOrDefault();
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
                                hash = PasteHelper.HashPassword(paste.Key, password);
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

                        // Delete the old file
                        string subDir = paste.FileName[0].ToString();
                        string filePath = Path.Combine(_config.PasteConfig.PasteDirectory, subDir, paste.FileName);
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);

                        // Generate a unique file name that does not currently exist
                        string newFilePath = FileHelper.GenerateRandomFileName(_config.PasteConfig.PasteDirectory, _config.PasteConfig.FileExtension, 10);
                        string fileName = Path.GetFileName(newFilePath);

                        string key = PasteHelper.GenerateKey(_config.PasteConfig.KeySize);
                        string iv = PasteHelper.GenerateIV(_config.PasteConfig.BlockSize);

                        PasteHelper.EncryptContents(model.Content, newFilePath, password, key, iv, _config.PasteConfig.KeySize, _config.PasteConfig.ChunkSize);

                        paste.Key = key;
                        paste.KeySize = _config.PasteConfig.KeySize;
                        paste.IV = iv;
                        paste.BlockSize = _config.PasteConfig.BlockSize;

                        paste.HashedPassword = PasteHelper.HashPassword(paste.Key, password);
                        paste.FileName = fileName;
                        paste.Title = model.Title;
                        paste.Syntax = model.Syntax;
                        paste.DateEdited = DateTime.Now;

                        _dbContext.Entry(paste).State = EntityState.Modified;
                        _dbContext.SaveChanges();

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
        public IActionResult Delete(string id)
        {
            Models.Paste foundPaste = _dbContext.Pastes.Where(p => p.Url == id).FirstOrDefault();
            if (foundPaste != null)
            {
                if (foundPaste.User.Username == User.Identity.Name)
                {
                    string filePath = foundPaste.FileName;
                    // Delete from the DB
                    _dbContext.Pastes.Remove(foundPaste);
                    _dbContext.SaveChanges();

                    // Delete the File
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    return Json(new { result = true, redirect = Url.SubRouteUrl("p", "Paste.Index") });
                }
                return Json(new { error = new { message = "You do not have permission to edit this Paste" } });
            }
            return Json(new { error = new { message = "This Paste does not exist" } });
        }

        private void CachePassword(string url, string password)
        {
            if (HttpContext != null)
            {
                HttpContext.Session.Set("PastePassword_" + url, password);
            }
        }

        private string GetCachedPassword(string url)
        {
            if (HttpContext != null)
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