using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Teknik.Areas.Paste;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Areas.Vault.Models;
using Teknik.Areas.Vault.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;
using Teknik.Models;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;

namespace Teknik.Areas.Vault.Controllers
{
    [Authorize]
    [Area("Vault")]
    public class VaultController : DefaultController
    {
        public VaultController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public async Task<IActionResult> ViewVault(string id)
        {
            Models.Vault foundVault = _dbContext.Vaults.Where(v => v.Url == id).FirstOrDefault();
            if (foundVault != null)
            {
                // Update view count
                foundVault.Views += 1;
                _dbContext.Entry(foundVault).State = EntityState.Modified;
                _dbContext.SaveChanges();

                ViewBag.Title = foundVault.Title + " | Vault";

                VaultViewModel model = new VaultViewModel();
                model.CurrentSub = Subdomain;

                model.Url = foundVault.Url;
                model.UserId = foundVault.UserId;
                model.User = foundVault.User;
                model.Title = foundVault.Title;
                model.Description = foundVault.Description;
                model.DateCreated = foundVault.DateCreated;
                model.DateEdited = foundVault.DateEdited;

                if (foundVault.VaultItems.Any())
                {
                    foreach (VaultItem item in foundVault.VaultItems.OrderBy(v => v.Index))
                    {
                        if (item.GetType().BaseType == typeof(UploadVaultItem))
                        {
                            UploadVaultItem upload = (UploadVaultItem)item;
                            // Increment Views
                            upload.Upload.Downloads += 1;
                            _dbContext.Entry(upload.Upload).State = EntityState.Modified;
                            _dbContext.SaveChanges();

                            UploadItemViewModel uploadModel = new UploadItemViewModel();
                            uploadModel.VaultItemId = item.VaultItemId;
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
                            _dbContext.Entry(paste.Paste).State = EntityState.Modified;
                            _dbContext.SaveChanges();

                            // Check Expiration
                            if (PasteHelper.CheckExpiration(paste.Paste))
                            {
                                _dbContext.Pastes.Remove(paste.Paste);
                                _dbContext.SaveChanges();
                                break;
                            }

                            PasteItemViewModel pasteModel = new PasteItemViewModel();
                            pasteModel.VaultItemId = item.VaultItemId;
                            pasteModel.Title = item.Title;
                            pasteModel.Description = item.Description;
                            pasteModel.DateAdded = item.DateAdded;

                            pasteModel.PasteId = paste.Paste.PasteId;
                            pasteModel.Url = paste.Paste.Url;
                            pasteModel.DatePosted = paste.Paste.DatePosted;
                            pasteModel.Syntax = paste.Paste.Syntax;
                            pasteModel.HasPassword = !string.IsNullOrEmpty(paste.Paste.HashedPassword);

                            if (!pasteModel.HasPassword)
                            {
                                // Read in the file
                                string subDir = paste.Paste.FileName[0].ToString();
                                string filePath = Path.Combine(_config.PasteConfig.PasteDirectory, subDir, paste.Paste.FileName);
                                if (!System.IO.File.Exists(filePath))
                                    continue;

                                byte[] ivBytes = Encoding.Unicode.GetBytes(paste.Paste.IV);
                                byte[] keyBytes = AesCounterManaged.CreateKey(paste.Paste.Key, ivBytes, paste.Paste.KeySize);
                                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                using (AesCounterStream cs = new AesCounterStream(fs, false, keyBytes, ivBytes))
                                using (StreamReader sr = new StreamReader(cs, Encoding.Unicode))
                                {
                                    pasteModel.Content = await sr.ReadToEndAsync();
                                }
                            }

                            model.Items.Add(pasteModel);
                        }
                    }
                }

                return View(model);
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult NewVault()
        {
            ViewBag.Title = "Create Vault";
            ModifyVaultViewModel model = new ModifyVaultViewModel();
            model.CurrentSub = Subdomain;
            return View("~/Areas/Vault/Views/Vault/ModifyVault.cshtml", model);
        }

        [HttpGet]
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult NewVaultFromService(string type, string items)
        {
            ViewBag.Title = "Create Vault";
            ModifyVaultViewModel model = new ModifyVaultViewModel();
            model.CurrentSub = Subdomain;

            string decodedItems = HttpUtility.UrlDecode(items);
            string[] allURLs = decodedItems.Split(',');
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
        [TrackPageView]
        public IActionResult EditVault(string url, string type, string items)
        {
            ViewBag.Title = "Edit Vault";
            Vault.Models.Vault foundVault = _dbContext.Vaults.Where(v => v.Url == url).FirstOrDefault();
            if (foundVault != null)
            {
                if (foundVault.User.Username == User.Identity.Name)
                {
                    ViewBag.Title = "Edit Vault | " + foundVault.Title;

                    ModifyVaultViewModel model = new ModifyVaultViewModel();
                    model.CurrentSub = Subdomain;
                    model.isEdit = true;
                    model.vaultId = foundVault.VaultId;
                    model.title = foundVault.Title;
                    model.description = foundVault.Description;

                    int index = 0;
                    // Add all their existing items for the vault
                    foreach (VaultItem item in foundVault.VaultItems.OrderBy(v => v.Index))
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

                    // If they passed any new items in via the parameters, let's add them
                    if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(items))
                    {
                        string decodedItems = HttpUtility.UrlDecode(items);
                        string[] allItems = decodedItems.Split(',');
                        foreach (string newItem in allItems)
                        {
                            string[] urlInfo = newItem.Split(':');
                            string itemId = urlInfo[0];
                            string title = string.Empty;
                            if (urlInfo.GetUpperBound(0) >= 1)
                            {
                                // They also passed in the original filename, so let's use it as our title
                                title = urlInfo[1];
                            }
                            if (IsValidItem(type, itemId))
                            {
                                ModifyVaultItemViewModel item = new ModifyVaultItemViewModel();
                                item.isTemplate = false;
                                item.index = index;
                                item.title = title;
                                item.url = itemId;
                                item.type = type;
                                model.items.Add(item);
                                index++;
                            }
                        }
                    }

                    return View("~/Areas/Vault/Views/Vault/ModifyVault.cshtml", model);
                }
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult CreateVault(ModifyVaultViewModel model)
        {
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.title))
                {
                    Models.Vault newVault = new Models.Vault();
                    // Create a new ID
                    string url = StringHelper.RandomString(_config.VaultConfig.UrlLength);
                    while (_dbContext.Vaults.Where(v => v.Url == url).FirstOrDefault() != null)
                    {
                        url = StringHelper.RandomString(_config.VaultConfig.UrlLength);
                    }
                    newVault.Url = url;
                    newVault.DateCreated = DateTime.Now;
                    newVault.Title = model.title;
                    newVault.Description = model.description;
                    if (User.Identity.IsAuthenticated)
                    {
                        User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                        if (user != null)
                        {
                            newVault.UserId = user.UserId;
                        }
                    }

                    // Add/Verify items
                    if (model.items.Any())
                    {
                        int index = 0;
                        foreach (ModifyVaultItemViewModel item in model.items)
                        {
                            if (IsValidItem(item.type, item.url))
                            {
                                switch (item.type.ToLower())
                                {
                                    case "upload":
                                        UploadVaultItem newUpload = new UploadVaultItem();
                                        newUpload.Index = index;
                                        newUpload.DateAdded = DateTime.Now;
                                        newUpload.Title = item.title;
                                        newUpload.Description = item.description;
                                        newUpload.UploadId = _dbContext.Uploads.Where(u => u.Url == item.url).FirstOrDefault().UploadId;
                                        newVault.VaultItems.Add(newUpload);
                                        index++;
                                        break;
                                    case "paste":
                                        PasteVaultItem newPaste = new PasteVaultItem();
                                        newPaste.Index = index;
                                        newPaste.DateAdded = DateTime.Now;
                                        newPaste.Title = item.title;
                                        newPaste.Description = item.description;
                                        newPaste.PasteId = _dbContext.Pastes.Where(p => p.Url == item.url).FirstOrDefault().PasteId;
                                        newVault.VaultItems.Add(newPaste);
                                        index++;
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
                    _dbContext.Vaults.Add(newVault);
                    _dbContext.SaveChanges();
                    return Json(new { result = new { url = Url.SubRouteUrl("v", "Vault.ViewVault", new { id = url }) } });
                }
                return Json(new { error = new { message = "You must supply a Title" } });
            }
            return Json(new { error = new { message = "Invalid Parameters" } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditVault(ModifyVaultViewModel model)
        {
            if (model != null)
            {
                Vault.Models.Vault foundVault = _dbContext.Vaults.Where(v => v.VaultId == model.vaultId).FirstOrDefault();
                if (foundVault != null)
                {
                    if (foundVault.User.Username == User.Identity.Name)
                    {
                        foundVault.DateEdited = DateTime.Now;
                        foundVault.Title = model.title;
                        foundVault.Description = model.description;

                        // Clear previous items
                        List<VaultItem> vaultItems = _dbContext.VaultItems.Where(v => v.VaultId == foundVault.VaultId).ToList();
                        if (vaultItems != null)
                        {
                            foreach (VaultItem item in vaultItems)
                            {
                                _dbContext.VaultItems.Remove(item);
                            }
                        }
                        foundVault.VaultItems.Clear();

                        // Add/Verify items
                        if (model.items.Any())
                        {
                            int index = 0;
                            foreach (ModifyVaultItemViewModel item in model.items)
                            {
                                if (IsValidItem(item.type, item.url))
                                {
                                    switch (item.type.ToLower())
                                    {
                                        case "upload":
                                            UploadVaultItem newUpload = new UploadVaultItem();
                                            newUpload.Index = index;
                                            newUpload.DateAdded = DateTime.Now;
                                            newUpload.Title = item.title;
                                            newUpload.Description = item.description;
                                            newUpload.UploadId = _dbContext.Uploads.Where(u => u.Url == item.url).FirstOrDefault().UploadId;
                                            foundVault.VaultItems.Add(newUpload);
                                            index++;
                                            break;
                                        case "paste":
                                            PasteVaultItem newPaste = new PasteVaultItem();
                                            newPaste.Index = index;
                                            newPaste.DateAdded = DateTime.Now;
                                            newPaste.Title = item.title;
                                            newPaste.Description = item.description;
                                            newPaste.PasteId = _dbContext.Pastes.Where(p => p.Url == item.url).FirstOrDefault().PasteId;
                                            foundVault.VaultItems.Add(newPaste);
                                            index++;
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

                        _dbContext.Entry(foundVault).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        return Json(new { result = new { url = Url.SubRouteUrl("v", "Vault.ViewVault", new { id = foundVault.Url }) } });
                    }
                    return Json(new { error = new { message = "You do not have permission to edit this Vault" } });
                }
                return Json(new { error = new { message = "That Vault does not exist" } });
            }
            return Json(new { error = new { message = "Invalid Parameters" } });
        }

        [HttpPost]
        [HttpOptions]
        public IActionResult Delete(string id)
        {
            Vault.Models.Vault foundVault = _dbContext.Vaults.Where(v => v.Url == id).FirstOrDefault();
            if (foundVault != null)
            {
                if (foundVault.User.Username == User.Identity.Name)
                {
                    _dbContext.Vaults.Remove(foundVault);
                    _dbContext.SaveChanges();

                    return Json(new { result = new { url = Url.SubRouteUrl("vault", "Vault.CreateVault") } });
                }
                return Json(new { error = new { message = "You do not have permission to edit this Vault" } });
            }
            return Json(new { error = new { message = "That Vault does not exist" } });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ValidateItem(string type, string url)
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
                        Upload.Models.Upload foundUpload = _dbContext.Uploads.Where(u => u.Url == url).FirstOrDefault();
                        if (foundUpload != null)
                        {
                            valid = true;
                        }
                        break;
                    case "paste":
                        Paste.Models.Paste foundPaste = _dbContext.Pastes.Where(p => p.Url == url).FirstOrDefault();
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
