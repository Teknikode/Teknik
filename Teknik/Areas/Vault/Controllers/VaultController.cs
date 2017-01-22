using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Attributes;
using Teknik.Controllers;

namespace Teknik.Areas.Vault.Controllers
{
    [TeknikAuthorize]
    public class VaultController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ViewVault(string id)
        {
            return View();
        }
    }
}