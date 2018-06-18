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

namespace Teknik.Areas.Paste.Controllers
{
    [TeknikAuthorize]
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
        public IActionResult ViewPaste(string type, string url, string password)
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
                model.Content = paste.Content;
                model.Title = paste.Title;
                model.Syntax = paste.Syntax;
                model.DatePosted = paste.DatePosted;

                byte[] data = Encoding.UTF8.GetBytes(paste.Content);

                if (User.Identity.IsAuthenticated && type.ToLower() == "full")
                {
                    Users.Models.User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        model.Vaults = user.Vaults.ToList();
                    }
                }

                // The paste has a password set
                if (!string.IsNullOrEmpty(paste.HashedPassword))
                {
                    string hash = string.Empty;
                    if (!string.IsNullOrEmpty(password))
                    {
                        byte[] passBytes = SHA384.Hash(paste.Key, password);
                        hash = passBytes.ToHex();
                        // We need to convert old pastes to the new password scheme
                        if (paste.Transfers.ToList().Exists(t => t.Type == TransferTypes.ASCIIPassword))
                        {
                            hash = Encoding.ASCII.GetString(passBytes);
                            // Remove the transfer types
                            paste.Transfers.Clear();
                            _dbContext.Entry(paste).State = EntityState.Modified;
                            _dbContext.SaveChanges();
                        }
                    }
                    if (string.IsNullOrEmpty(password) || hash != paste.HashedPassword)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
                        passModel.Url = url;
                        passModel.Type = type;
                        // Redirect them to the password request page
                        return View("~/Areas/Paste/Views/Paste/PasswordNeeded.cshtml", passModel);
                    }

                    data = Convert.FromBase64String(paste.Content);
                    // Now we decrypt the content
                    byte[] ivBytes = Encoding.Unicode.GetBytes(paste.IV);
                    byte[] keyBytes = AesCounterManaged.CreateKey(password, ivBytes, paste.KeySize);
                    data = AesCounterManaged.Decrypt(data, keyBytes, ivBytes);
                    model.Content = Encoding.Unicode.GetString(data);
                }

                if (type.ToLower() == "full" || type.ToLower() == "simple")
                {
                    // Transform content into HTML
                    //if (!Highlighter.Lexers.ToList().Exists(l => l.Aliases.Contains(model.Syntax)))
                    //{
                    //    model.Syntax = "text";
                    //}
                    //Highlighter highlighter = new Highlighter();
                    // Add a space in front of the content due to bug with pygment (No idea why yet)
                    model.Content = model.Content;//highlighter.HighlightToHtml(" " + model.Content, model.Syntax, _config.PasteConfig.SyntaxVisualStyle, generateInlineStyles: true, fragment: true);
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
                            FileName = url,
                            Inline = true
                        };

                        Response.Headers.Add("Content-Disposition", cd.ToString());

                        return File(data, "application/octet-stream");
                    default:
                        return View("~/Areas/Paste/Views/Paste/Full.cshtml", model);
                }
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [AllowAnonymous]
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
    }
}