using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Privacy.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Privacy.Controllers
{
    public class PrivacyController : DefaultController
    {
        // GET: Privacy/Privacy
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Privacy Policy - " + Config.Title;
            ViewBag.Description = "Teknik privacy policy.";

            return View(new PrivacyViewModel());
        }
    }
}