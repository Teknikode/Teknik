using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Teknik.Pygments;
using Teknik.Areas.Error.Controllers;
using Teknik.Areas.Paste.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Paste.Controllers
{
    public class PasteController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Paste - " + Config.Title;
            ViewBag.Description = "Paste your code or text easily and securely.  Set an expiration, set a password, or leave it open for the world to see.";
            PasteCreateViewModel model = new PasteCreateViewModel();
            return View(model);
        }

        [TrackDownload]
        [AllowAnonymous]
        public ActionResult ViewPaste(string type, string url, string password)
        {
            Models.Paste paste = db.Pastes.Where(p => p.Url == url).FirstOrDefault();
            if (paste != null)
            {
                ViewBag.Title = ((string.IsNullOrEmpty(paste.Title)) ? string.Empty : paste.Title + " - ") + Config.Title + " Paste";
                ViewBag.Description = "Paste your code or text easily and securely.  Set an expiration, set a password, or leave it open for the world to see.";
                // Increment Views
                paste.Views += 1;
                db.Entry(paste).State = EntityState.Modified;
                db.SaveChanges();

                // Check Expiration
                if (PasteHelper.CheckExpiration(paste))
                {
                    db.Pastes.Remove(paste);
                    db.SaveChanges();
                    return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
                }

                PasteViewModel model = new PasteViewModel();
                model.Url = url;
                model.Content = paste.Content;
                model.Title = paste.Title;
                model.Syntax = paste.Syntax;
                model.DatePosted = paste.DatePosted;

                byte[] data = Encoding.UTF8.GetBytes(paste.Content);

                // The paste has a password set
                if (!string.IsNullOrEmpty(paste.HashedPassword))
                {
                    string hash = string.Empty;
                    if (!string.IsNullOrEmpty(password))
                    {
                        byte[] passBytes = Utilities.SHA384.Hash(paste.Key, password);
                        hash = passBytes.ToHex();
                        // We need to convert old pastes to the new password scheme
                        if (paste.Transfers.ToList().Exists(t => t.Type == TransferTypes.ASCIIPassword))
                        {
                            hash = Encoding.ASCII.GetString(passBytes);
                            // Remove the transfer types
                            paste.Transfers.Clear();
                            db.Entry(paste).State = EntityState.Modified;
                            db.SaveChanges();
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
                    byte[] keyBytes = AES.CreateKey(password, ivBytes, paste.KeySize);
                    data = AES.Decrypt(data, keyBytes, ivBytes);
                    model.Content = Encoding.Unicode.GetString(data);
                }

                if (type.ToLower() == "full" || type.ToLower() == "simple")
                {
                    // Transform content into HTML
                    if (!Highlighter.Lexers.ToList().Exists(l => l.Aliases.Contains(model.Syntax)))
                    {
                        model.Syntax = "text";
                    }
                    Highlighter highlighter = new Highlighter();
                    // Add a space in front of the content due to bug with pygment (No idea why yet)
                    model.Content = highlighter.HighlightToHtml(" " + model.Content, model.Syntax, Config.PasteConfig.SyntaxVisualStyle, generateInlineStyles: true, fragment: true);
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

                        Response.AppendHeader("Content-Disposition", cd.ToString());

                        return File(data, "application/octet-stream");
                    default:
                        return View("~/Areas/Paste/Views/Paste/Full.cshtml", model);
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Paste([Bind(Include = "Content, Title, Syntax, ExpireLength, ExpireUnit, Password, Hide")]PasteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Config.PasteConfig.Enabled)
                {
                    try
                    {
                        Models.Paste paste = PasteHelper.CreatePaste(model.Content, model.Title, model.Syntax, model.ExpireUnit, model.ExpireLength ?? 1, model.Password, model.Hide);

                        if (model.ExpireUnit == "view")
                        {
                            paste.Views = -1;
                        }

                        if (User.Identity.IsAuthenticated)
                        {
                            Users.Models.User user = UserHelper.GetUser(db, User.Identity.Name);
                            if (user != null)
                            {
                                paste.UserId = user.UserId;
                            }
                        }

                        db.Pastes.Add(paste);
                        db.SaveChanges();

                        return Redirect(Url.SubRouteUrl("p", "Paste.View", new { type = "Full", url = paste.Url }));
                    }
                    catch (Exception ex)
                    {
                        return Redirect(Url.SubRouteUrl("error", "Error.500", new { exception = ex }));
                    }
                }
                Redirect(Url.SubRouteUrl("error", "Error.Http403"));
            }
            return View("~/Areas/Paste/Views/Paste/Index.cshtml", model);
        }
    }
}