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
                if (CheckExpiration(paste))
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
                        passModel.CallingAction = Url.SubRouteUrl("paste", "Paste.View", new { type = type });
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
                    Models.Paste paste = db.Pastes.Create();
                    paste.DatePosted = DateTime.Now;
                    paste.Url = Utility.RandomString(Config.PasteConfig.UrlLength);
                    paste.MaxViews = 0;
                    paste.Views = -1;

                    // Figure out the expire date (null if 'never' or 'visit')
                    if (model.ExpireLength.HasValue || model.ExpireUnit == "never")
                    {
                        switch (model.ExpireUnit)
                        {
                            case "never":
                                break;
                            case "view":
                                paste.MaxViews = model.ExpireLength ?? 0;
                                break;
                            case "minute":
                                paste.ExpireDate = paste.DatePosted.AddMinutes(model.ExpireLength ?? 1);
                                break;
                            case "hour":
                                paste.ExpireDate = paste.DatePosted.AddHours(model.ExpireLength ?? 1);
                                break;
                            case "day":
                                paste.ExpireDate = paste.DatePosted.AddDays(model.ExpireLength ?? 1);
                                break;
                            case "month":
                                paste.ExpireDate = paste.DatePosted.AddMonths(model.ExpireLength ?? 1);
                                break;
                            case "year":
                                paste.ExpireDate = paste.DatePosted.AddYears(model.ExpireLength ?? 1);
                                break;
                            default:
                                break;
                        }
                    }

                    // Set the hashed password if one is provided and encrypt stuff
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        string key = Utility.RandomString(Config.PasteConfig.KeySize / 8);
                        string iv = Utility.RandomString(Config.PasteConfig.BlockSize / 8);
                        paste.HashedPassword = Helpers.SHA384.Hash(key, model.Password);

                        // Encrypt Content
                        byte[] data = Encoding.Unicode.GetBytes(model.Content);
                        byte[] ivBytes = Encoding.Unicode.GetBytes(iv);
                        byte[] keyBytes = AES.CreateKey(model.Password, ivBytes, Config.PasteConfig.KeySize);
                        byte[] encData = AES.Encrypt(data, keyBytes, ivBytes);
                        model.Content = Encoding.Unicode.GetString(encData);

                        paste.Key = key;
                        paste.KeySize = Config.PasteConfig.KeySize;
                        paste.IV = iv;
                        paste.BlockSize = Config.PasteConfig.BlockSize;
                    }

                    paste.Content = model.Content;
                    paste.Title = model.Title;
                    paste.Syntax = model.Syntax;
                    paste.Hide = model.Hide;

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

        private bool CheckExpiration(Models.Paste paste)
        {
            if (paste.ExpireDate != null && DateTime.Now >= paste.ExpireDate)
                return true;
            if (paste.MaxViews > 0 && paste.Views > paste.MaxViews)
                return true;

            return false;
        }
    }
}