using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Stream.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Stream.Controllers
{
    public class StreamController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Teknikam";
            StreamViewModel model = new StreamViewModel();
            return View(model);
        }
    }
}