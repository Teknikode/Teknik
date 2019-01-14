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
            ViewBag.Title = "Paste - " + _config.Title;
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
                ViewBag.Title = ((string.IsNullOrEmpty(paste.Title)) ? string.Empty : paste.Title + " - ") + _config.Title + " Paste";
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

                if (User.Identity.IsAuthenticated && type.ToLower() == "full")
                {
                    Users.Models.User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        model.Vaults = user.Vaults.ToList();
                    }
                }

                byte[] ivBytes = Encoding.Unicode.GetBytes(paste.IV);
                byte[] keyBytes = AesCounterManaged.CreateKey(paste.Key, ivBytes, paste.KeySize);

                // The paste has a password set
                if (!string.IsNullOrEmpty(paste.HashedPassword))
                {
                    string hash = string.Empty;
                    if (!string.IsNullOrEmpty(password))
                    {
                        hash = PasteHelper.HashPassword(paste.Key, password);
                        keyBytes = AesCounterManaged.CreateKey(password, ivBytes, paste.KeySize);
                    }
                    if (string.IsNullOrEmpty(password) || hash != paste.HashedPassword)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
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

                // Read in the file
                string subDir = paste.FileName[0].ToString();
                string filePath = Path.Combine(_config.PasteConfig.PasteDirectory, subDir, paste.FileName);
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
        public IActionResult Paste([Bind("Content, Title, Syntax, ExpireLength, ExpireUnit, Password, Hide")]PasteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_config.PasteConfig.Enabled)
                {
                    try
                    {
                        Models.Paste paste = PasteHelper.CreatePaste(_config, _dbContext, model.Content, model.Title, model.Syntax, model.ExpireUnit, model.ExpireLength ?? 1, model.Password, model.Hide);

                        if (model.ExpireUnit == "view")
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

                    return Json(new { result = true });
                }
                return Json(new { error = new { message = "You do not have permission to edit this Paste" } });
            }
            return Json(new { error = new { message = "This Paste does not exist" } });
        }
    }
}