using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.FAQ.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;

namespace Teknik.Areas.FAQ.Controllers
{
    [TeknikAuthorize]
    public class FAQController : DefaultController
    {
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Frequently Asked Questions - " + Config.Title;
            FAQViewModel model = new FAQViewModel();
            return View(model);
        }
    }
}