using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Controllers;

namespace Teknik.Areas.About.Controllers
{
    public class AboutController : DefaultController
    {
        [AllowAnonymous]
        // GET: About/About
        public ActionResult Index()
        {
            ViewBag.Title = "About - " + Config.Title;
            ViewBag.Message = "What is Teknik?";

            return View();
        }
    }
}