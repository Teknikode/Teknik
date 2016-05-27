using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.About.ViewModels;
using Teknik.Controllers;
using Teknik.Filters;

namespace Teknik.Areas.About.Controllers
{
    public class AboutController : DefaultController
    {
        [AllowAnonymous]
        [TrackPageView]
        public ActionResult Index()
        {
            ViewBag.Title = "About - " + Config.Title;
            ViewBag.Description = "What is Teknik?";

            return View(new AboutViewModel());
        }
    }
}