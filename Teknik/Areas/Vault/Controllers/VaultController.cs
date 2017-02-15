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
                // Update view count
                foundVault.Views += 1;
                db.Entry(foundVault).State = EntityState.Modified;
                db.SaveChanges();

                ViewBag.Title = foundVault.Title + " - Teknik Vault";

                VaultViewModel model = new VaultViewModel();

                model.Url = foundVault.Url;
                model.UserId = foundVault.UserId;
                model.User = foundVault.User;
                model.Title = foundVault.Title;
                model.Description = foundVault.Description;
                model.DateCreated = foundVault.DateCreated;
                model.DateEdited = foundVault.DateEdited;

                if (foundVault.VaultItems.Any())
                {
                    foreach (VaultItem item in foundVault.VaultItems)
                    {
                        VaultItemViewModel itemModel = new VaultItemViewModel();
                        itemModel.Title = item.Title;
                        itemModel.Description = item.Description;
                        itemModel.DateAdded = item.DateAdded;

                        if (item.GetType().BaseType == typeof(UploadVaultItem))
                        {
                            UploadVaultItem upload = (UploadVaultItem)item;
                            // Increment Views
                            upload.Upload.Downloads += 1;
                            db.Entry(upload.Upload).State = EntityState.Modified;
                            db.SaveChanges();

                            UploadItemViewModel uploadModel = new UploadItemViewModel();
                            uploadModel.Title = item.Title;
                            uploadModel.Description = item.Description;
                            uploadModel.DateAdded = item.DateAdded;
                            uploadModel.Upload = upload.Upload;
                            model.Items.Add(uploadModel);
                        }
                        else if (item.GetType().BaseType == typeof(PasteVaultItem))
                        {
                            PasteVaultItem paste = (PasteVaultItem)item;
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

                            PasteItemViewModel pasteModel = new PasteItemViewModel();
                            pasteModel.Title = item.Title;
                            pasteModel.Description = item.Description;
                            pasteModel.DateAdded = item.DateAdded;
                            pasteModel.Paste = paste.Paste;
                            model.Items.Add(pasteModel);
                        }
                    }
                }

                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult NewVault()
        {
            ViewBag.Title = "Create Vault";
            ModifyVaultViewModel model = new ModifyVaultViewModel();
            return View("~/Areas/Vault/Views/Vault/ModifyVault.cshtml", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult NewVaultFromService(string type, string urls)
        {
            ViewBag.Title = "Create Vault";
            ModifyVaultViewModel model = new ModifyVaultViewModel();

            string[] allURLs = urls.Split(',');
            int index = 0;
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
                    ModifyVaultItemViewModel item = new ModifyVaultItemViewModel();
                    item.isTemplate = false;
                    item.index = index;
                    item.title = title;
                    item.url = uploadId;
                    item.type = type;
                    model.items.Add(item);
                    index++;
                }
            }

            return View("~/Areas/Vault/Views/Vault/ModifyVault.cshtml", model);
        }

        [HttpGet]
        public ActionResult EditVault(string url)
        {
            ViewBag.Title = "Edit Vault";
            Vault.Models.Vault foundVault = db.Vaults.Where(v => v.Url == url).FirstOrDefault();
            if (foundVault != null)
            {
                if (foundVault.User.Username == User.Identity.Name)
                {
                    ViewBag.Title = "Edit Vault - " + foundVault.Title;

                    ModifyVaultViewModel model = new ModifyVaultViewModel();
                    model.isEdit = true;
                    model.vaultId = foundVault.VaultId;
                    model.title = foundVault.Title;
                    model.description = foundVault.Description;

                    int index = 0;
                    foreach (VaultItem item in foundVault.VaultItems)
                    {
                        ModifyVaultItemViewModel itemModel = new ModifyVaultItemViewModel();
                        itemModel.index = index;
                        itemModel.isTemplate = false;

                        if (item.GetType().BaseType == typeof(UploadVaultItem))
                        {
                            UploadVaultItem upload = (UploadVaultItem)item;
                            itemModel.title = upload.Title;
                            itemModel.description = upload.Description;
                            itemModel.type = "Upload";
                            itemModel.url = upload.Upload.Url;
                            model.items.Add(itemModel);
                            index++;
                        }
                        else if (item.GetType().BaseType == typeof(PasteVaultItem))
                        {
                            PasteVaultItem paste = (PasteVaultItem)item;
                            itemModel.title = paste.Title;
                            itemModel.description = paste.Description;
                            itemModel.type = "Paste";
                            itemModel.url = paste.Paste.Url;
                            model.items.Add(itemModel);
                            index++;
                        }
                    }

                    return View("~/Areas/Vault/Views/Vault/ModifyVault.cshtml", model);
                }
                return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateVault(ModifyVaultViewModel model)
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
                        foreach (ModifyVaultItemViewModel item in model.items)
                        {
                            if (IsValidItem(item.type, item.url))
                            {
                                switch (item.type.ToLower())
                                {
                                    case "upload":
                                        UploadVaultItem newUpload = new UploadVaultItem();
                                        newUpload.DateAdded = DateTime.Now;
                                        newUpload.Title = item.title;
                                        newUpload.Description = item.description;
                                        newUpload.UploadId = db.Uploads.Where(u => u.Url == item.url).FirstOrDefault().UploadId;
                                        newVault.VaultItems.Add(newUpload);
                                        break;
                                    case "paste":
                                        PasteVaultItem newPaste = new PasteVaultItem();
                                        newPaste.DateAdded = DateTime.Now;
                                        newPaste.Title = item.title;
                                        newPaste.Description = item.description;
                                        newPaste.PasteId = db.Pastes.Where(p => p.Url == item.url).FirstOrDefault().PasteId;
                                        newVault.VaultItems.Add(newPaste);
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
        [ValidateAntiForgeryToken]
        public ActionResult EditVault(ModifyVaultViewModel model)
        {
            if (model != null)
            {
                Vault.Models.Vault foundVault = db.Vaults.Where(v => v.VaultId == model.vaultId).FirstOrDefault();
                if (foundVault != null)
                {
                    if (foundVault.User.Username == User.Identity.Name)
                    {
                        foundVault.DateEdited = DateTime.Now;
                        foundVault.Title = model.title;
                        foundVault.Description = model.description;

                        // Clear previous items
                        List<VaultItem> vaultItems = db.VaultItems.Where(v => v.VaultId == foundVault.VaultId).ToList();
                        if (vaultItems != null)
                        {
                            foreach (VaultItem item in vaultItems)
                            {
                                db.VaultItems.Remove(item);
                            }
                        }
                        foundVault.VaultItems.Clear();

                        // Add/Verify items
                        if (model.items.Any())
                        {
                            foreach (ModifyVaultItemViewModel item in model.items)
                            {
                                if (IsValidItem(item.type, item.url))
                                {
                                    switch (item.type.ToLower())
                                    {
                                        case "upload":
                                            UploadVaultItem newUpload = new UploadVaultItem();
                                            newUpload.DateAdded = DateTime.Now;
                                            newUpload.Title = item.title;
                                            newUpload.Description = item.description;
                                            newUpload.UploadId = db.Uploads.Where(u => u.Url == item.url).FirstOrDefault().UploadId;
                                            foundVault.VaultItems.Add(newUpload);
                                            break;
                                        case "paste":
                                            PasteVaultItem newPaste = new PasteVaultItem();
                                            newPaste.DateAdded = DateTime.Now;
                                            newPaste.Title = item.title;
                                            newPaste.Description = item.description;
                                            newPaste.PasteId = db.Pastes.Where(p => p.Url == item.url).FirstOrDefault().PasteId;
                                            foundVault.VaultItems.Add(newPaste);
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

                        db.Entry(foundVault).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json(new { result = new { url = Url.SubRouteUrl("v", "Vault.ViewVault", new { id = foundVault.Url }) } });
                    }
                    return Json(new { error = new { message = "You do not have permission to edit this Vault" } });
                }
                return Json(new { error = new { message = "That Vault does not exist" } });
            }
            return Json(new { error = new { message = "Invalid Parameters" } });
        }

        [HttpPost]
        public ActionResult DeleteVault(string url)
        {
            Vault.Models.Vault foundVault = db.Vaults.Where(v => v.Url == url).FirstOrDefault();
            if (foundVault != null)
            {
                if (foundVault.User.Username == User.Identity.Name)
                {
                    db.Vaults.Remove(foundVault);
                    db.SaveChanges();

                    return Json(new { result = new { url = Url.SubRouteUrl("vault", "Vault.CreateVault") } });
                }
                return Json(new { error = new { message = "You do not have permission to edit this Vault" } });
            }
            return Json(new { error = new { message = "That Vault does not exist" } });
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