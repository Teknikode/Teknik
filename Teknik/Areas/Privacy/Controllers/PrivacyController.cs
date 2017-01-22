using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Privacy.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;

namespace Teknik.Areas.Privacy.Controllers
{
    [TeknikAuthorize]
    public class PrivacyController : DefaultController
    {
        // GET: Privacy/Privacy
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Privacy Policy - " + Config.Title;
            ViewBag.Description = "Teknik privacy policy.";

            return View(new PrivacyViewModel());
        }
    }
}