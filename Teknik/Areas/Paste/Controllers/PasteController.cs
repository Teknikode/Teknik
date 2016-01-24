using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Error.Controllers;
using Teknik.Areas.Paste.ViewModels;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;


namespace Teknik.Areas.Paste.Controllers
{
    public class PasteController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = Config.Title + " Paste";
            PasteCreateViewModel model = new PasteCreateViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ViewPaste(string type, string url, string password)
        {
            Models.Paste paste = db.Pastes.Where(p => p.Url == url).FirstOrDefault();
            if (paste != null)
            {
                ViewBag.Title = ((string.IsNullOrEmpty(paste.Title)) ? string.Empty : paste.Title + " - ") + Config.Title + " Paste";
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

                byte[] data = Encoding.Unicode.GetBytes(paste.Content);

                // The paste has a password set
                if (!string.IsNullOrEmpty(paste.HashedPassword))
                {
                    if (string.IsNullOrEmpty(password) || Helpers.SHA384.Hash(paste.Key, password) != paste.HashedPassword)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
                        passModel.Url = url;
                        passModel.Type = type;
                        // Redirect them to the password request page
                        return View("~/Areas/Paste/Views/Paste/PasswordNeeded.cshtml", passModel);
                    }
                    // Now we decrypt the content
                    byte[] ivBytes = Encoding.Unicode.GetBytes(paste.IV);
                    byte[] keyBytes = AES.CreateKey(password, ivBytes, paste.KeySize);
                    data = AES.Decrypt(data, keyBytes, ivBytes);
                    model.Content = Encoding.Unicode.GetString(data);
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
        [ValidateAntiForgeryToken]
        public ActionResult Paste([Bind(Include = "Content, Title, Syntax, ExpireLength, ExpireUnit, Password, Hide")]PasteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Models.Paste paste = PasteHelper.CreatePaste(model.Content, model.Title, model.Syntax, model.ExpireUnit, model.ExpireLength ?? 1, model.Password, model.Hide);

                    db.Pastes.Add(paste);
                    db.SaveChanges();

                    return Redirect(Url.SubRouteUrl("paste", "Paste.View", new { type = "Full", url = paste.Url, password = model.Password }));
                }
                catch (Exception ex)
                {
                    return Redirect(Url.SubRouteUrl("error", "Error.500", new { exception = ex }));
                }
            }
            return View("~/Areas/Paste/Views/Paste/Index.cshtml", model);
        }
    }
}