using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
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
            PasteViewModel model = new PasteViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ViewPaste(string url, string pass)
        {
            Models.Paste paste = db.Pastes.Where(p => p.Url == url).FirstOrDefault();
            if (paste != null)
            {
                PasteViewModel model = new PasteViewModel();
                model.Url = url;
                model.Content = paste.Content;
                model.Title = paste.Title;
                model.Syntax = paste.Syntax;
                model.DatePosted = paste.DatePosted;

                // The paste has a password set
                if (!string.IsNullOrEmpty(paste.HashedPassword))
                {
                    if (string.IsNullOrEmpty(pass) || Helpers.SHA384.Hash(paste.Key, pass) != paste.HashedPassword)
                    {
                        PasswordViewModel passModel = new PasswordViewModel();
                        passModel.Url = url;
                        passModel.CallingView = "ViewPaste";
                        // Redirect them to the password request page
                        return View("~/Areas/Paste/Views/Paste/PasswordNeeded", passModel);
                    }
                    // Now we decrypt the content
                    byte[] data = Encoding.UTF8.GetBytes(paste.Content);
                    byte[] ivBytes = Encoding.UTF8.GetBytes(paste.IV);
                    byte[] encData = AES.Decrypt(data, AES.CreateKey(pass, paste.IV, paste.KeySize), ivBytes);
                    model.Content = Encoding.UTF8.GetString(encData);
                }

                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [AllowAnonymous]
        public ActionResult Simple(string url, string pass)
        {
            PasteViewModel model = new PasteViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Raw(string url, string pass)
        {
            PasteViewModel model = new PasteViewModel();
            return View(model);

            // Create File
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = upload.Url,
                Inline = true
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(data, upload.ContentType);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Paste(string content, string title, string syntax, string password, bool hide = false)
        {
            Models.Paste paste = db.Pastes.Create();
            paste.DatePosted = DateTime.Now;
            paste.Url = Utility.RandomString(Config.PasteConfig.UrlLength);

            // Set the hashed password if one is provided and encrypt stuff
            if (!string.IsNullOrEmpty(password))
            {
                string key = Utility.RandomString(Config.PasteConfig.KeySize / 8);
                string iv = Utility.RandomString(Config.PasteConfig.BlockSize / 8);
                paste.HashedPassword = Helpers.SHA384.Hash(key, password);

                // Encrypt Content
                byte[] data = Encoding.UTF8.GetBytes(content);
                byte[] keyBytes = AES.CreateKey(password, iv, Config.PasteConfig.KeySize);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
                byte[] encData = AES.Encrypt(data, keyBytes, ivBytes);
                content = Encoding.UTF8.GetString(encData);

                paste.Key = key;
                paste.KeySize = Config.PasteConfig.KeySize;
                paste.IV = iv;
                paste.BlockSize = Config.PasteConfig.BlockSize;
            }

            paste.Content = content;
            paste.Title = title;
            paste.Syntax = syntax;

            db.Pastes.Add(paste);
            db.SaveChanges();

            return Redirect(Url.SubRouteUrl("paste", "Paste.View", new { url = paste.Url }));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitPassword(string url, string password)
        {
            Models.Paste paste = db.Pastes.Where(p => p.Url == url).FirstOrDefault();
            if (paste != null)
            {
                if (Helpers.SHA384.Hash(paste.Key, password) == paste.HashedPassword)
                {
                    return View(model);
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        private PasteViewModel GetPasteModel(Models.Paste paste, string pass)
        {
            PasteViewModel model = new PasteViewModel();
            model.Url = paste.Url;
            model.Content = paste.Content;
            model.Title = paste.Title;
            model.Syntax = paste.Syntax;
            model.DatePosted = paste.DatePosted;

            // The paste has a password set
            if (!string.IsNullOrEmpty(paste.HashedPassword))
            {
                if (string.IsNullOrEmpty(pass) || Helpers.SHA384.Hash(paste.Key, pass) != paste.HashedPassword)
                {
                    // Redirect them to password page
                }
                // Now we decrypt the content
                byte[] data = Encoding.UTF8.GetBytes(paste.Content);
                byte[] ivBytes = Encoding.UTF8.GetBytes(paste.IV);
                byte[] encData = AES.Decrypt(data, AES.CreateKey(pass, paste.IV, paste.KeySize), ivBytes);
                model.Content = Encoding.UTF8.GetString(encData);
            }

            return model;
        }
    }
}