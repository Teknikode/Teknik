using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Vault.Models;
using Teknik.Areas.Vault.ViewModels;
using Teknik.Attributes;
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
        public ActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ViewVault(string id)
        {
            Models.Vault foundVault = db.Vaults.Where(v => v.Url == id).FirstOrDefault();
            if (foundVault != null)
            {
                VaultViewModel model = new VaultViewModel();

                model.Url = foundVault.Url;
                model.Title = foundVault.Title;
                model.Description = foundVault.Description;
                model.DateCreated = foundVault.DateCreated;
                model.DateEdited = foundVault.DateEdited;
                model.Items = foundVault.Items.ToList();

                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }
    }
}