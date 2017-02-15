using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Paste;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Areas.Vault.Models;
using Teknik.Areas.Vault.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Areas.Vault.Controllers
{
    [TeknikAuthorize]
    public class VaultController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult ViewVault(string id)
        {
            Models.Vault foundVault = db.Vaults.Where(v => v.Url == id).FirstOrDefault();
            if (foundVault != null)
            {
                ViewBag.Title = foundVault.Title + " - Vault";

                VaultViewModel model = new VaultViewModel();

                model.Url = foundVault.Url;
                model.Title = foundVault.Title;
                model.Description = foundVault.Description;
                model.DateCreated = foundVault.DateCreated;
                model.DateEdited = foundVault.DateEdited;

                if (foundVault.Items.Any())
                {
                    foreach (VaultItem item in foundVault.Items)
                    {
                        if (item.GetType().BaseType == typeof(UploadItem))
                        {
                            UploadItem upload = (UploadItem)item;
                            // Increment Views
                            upload.Upload.Downloads += 1;
                            db.Entry(upload.Upload).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (item.GetType().BaseType == typeof(PasteItem))
                        {
                            PasteItem paste = (PasteItem)item;
                            // Increment Views
                            paste.Paste.Views += 1;
                            db.Entry(paste.Paste).State = EntityState.Modified;
                            db.SaveChanges();

                            // Check Expiration
                            if (PasteHelper.CheckExpiration(paste.Paste))
                            {
                                db.Pastes.Remove(paste.Paste);
                                db.SaveChanges();
                                break;
                            }
                        }

                        model.Items.Add(item);
                    }
                }
                model.Items = foundVault.Items.ToList();

                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult NewVault()
        {
            ViewBag.Title = "Create Vault";
            NewVaultViewModel model = new NewVaultViewModel();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult NewVaultFromService(string type, string urls)
        {
            ViewBag.Title = "Create Vault";
            NewVaultViewModel model = new NewVaultViewModel();

            string[] allURLs = urls.Split(',');
            foreach (string url in allURLs)
            {
                string[] urlInfo = url.Split(':');
                string uploadId = urlInfo[0];
                string title = string.Empty;
                if (urlInfo.GetUpperBound(0) >= 1)
                {
                    // They also passed in the original filename, so let's use it as our title
                    title = urlInfo[1];
                }
                if (IsValidItem(type, uploadId))
                {
                    NewVaultItemViewModel item = new NewVaultItemViewModel();
                    item.title = title;
                    item.url = uploadId;
                    item.type = type;
                    model.items.Add(item);
                }
            }

            return View("~/Areas/Vault/Views/Vault/NewVault.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateVault(NewVaultViewModel model)
        {
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.title))
                {
                    Vault.Models.Vault newVault = db.Vaults.Create();
                    // Create a new ID
                    string url = StringHelper.RandomString(Config.VaultConfig.UrlLength);
                    while (db.Vaults.Where(v => v.Url == url).FirstOrDefault() != null)
                    {
                        url = StringHelper.RandomString(Config.VaultConfig.UrlLength);
                    }
                    newVault.Url = url;
                    newVault.DateCreated = DateTime.Now;
                    newVault.Title = model.title;
                    newVault.Description = model.description;
                    if (User.Identity.IsAuthenticated)
                    {
                        User user = UserHelper.GetUser(db, User.Identity.Name);
                        if (user != null)
                        {
                            newVault.UserId = user.UserId;
                        }
                    }

                    // Add/Verify items
                    if (model.items.Any())
                    {
                        foreach (NewVaultItemViewModel item in model.items)
                        {
                            if (IsValidItem(item.type, item.url))
                            {
                                switch (item.type.ToLower())
                                {
                                    case "upload":
                                        UploadItem newUpload = new UploadItem();
                                        newUpload.DateAdded = DateTime.Now;
                                        newUpload.Title = item.title;
                                        newUpload.Description = item.description;
                                        newUpload.UploadId = db.Uploads.Where(u => u.Url == item.url).FirstOrDefault().UploadId;
                                        newVault.Items.Add(newUpload);
                                        break;
                                    case "paste":
                                        PasteItem newPaste = new PasteItem();
                                        newPaste.DateAdded = DateTime.Now;
                                        newPaste.Title = item.title;
                                        newPaste.Description = item.description;
                                        newPaste.PasteId = db.Pastes.Where(p => p.Url == item.url).FirstOrDefault().PasteId;
                                        newVault.Items.Add(newPaste);
                                        break;
                                    default:
                                        return Json(new { error = new { message = "You have an invalid item type: " + item.type } });
                                }
                            }
                            else
                            {
                                return Json(new { error = new { message = "You have an invalid item URL: " + item.url } });
                            }
                        }
                    }

                    // Add and save the new vault
                    db.Vaults.Add(newVault);
                    db.SaveChanges();
                    return Json(new { result = new { url = Url.SubRouteUrl("v", "Vault.ViewVault", new { id = url }) } });
                }
                return Json(new { error = new { message = "You must supply a Title" } });
            }
            return Json(new { error = new { message = "Invalid Parameters" } });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ValidateItem(string type, string url)
        {
            if (IsValidItem(type, url))
            {
                return Json(new { result = new { valid = true } });
            }
            else
            {
                return Json(new { error = new { message = "Invalid URL Id for this Item" } });
            }
        }

        private bool IsValidItem(string type, string url)
        {
            bool valid = false;
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(url))
            {
                switch (type.ToLower())
                {
                    case "upload":
                        Upload.Models.Upload foundUpload = db.Uploads.Where(u => u.Url == url).FirstOrDefault();
                        if (foundUpload != null)
                        {
                            valid = true;
                        }
                        break;
                    case "paste":
                        Paste.Models.Paste foundPaste = db.Pastes.Where(p => p.Url == url).FirstOrDefault();
                        if (foundPaste != null)
                        {
                            valid = true;
                        }
                        break;
                }
            }
            return valid;
        }
    }
}