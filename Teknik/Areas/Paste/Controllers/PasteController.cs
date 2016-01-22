using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Paste.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Paste.Controllers
{
    public class PasteController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            PasteViewModel model = new PasteViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Simple()
        {
            PasteViewModel model = new PasteViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Raw()
        {
            PasteViewModel model = new PasteViewModel();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Paste()
        {
            return View();
        }
    }
}