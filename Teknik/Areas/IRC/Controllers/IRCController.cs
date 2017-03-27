using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Controllers;
using Teknik.Attributes;
using Teknik.Areas.IRC.ViewModels;

namespace Teknik.Areas.IRC.Controllers
{
    [TeknikAuthorize]
    public class IRCController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Client()
        {
            ViewBag.Title = "Web Client - " + Config.Title + " IRC";
            ClientViewModel model = new ClientViewModel();
            return View(model);
        }
    }
}