using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Help.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Help.Controllers
{
    public class HelpController : DefaultController
    {
        // GET: Help/Help
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult API(string version, string service)
        {
            HelpViewModel model = new HelpViewModel();
            if (string.IsNullOrEmpty(version) && string.IsNullOrEmpty(service))
            {
                ViewBag.Title = "API Help - " + Config.Title;
                return View("~/Areas/Help/Views/Help/API/API.cshtml", model);
            }
            else if(!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(service))
            {
                ViewBag.Title = service + " API " + version + " Help - " + Config.Title;
                return View("~/Areas/Help/Views/Help/API/" + version + "/" + service + ".cshtml", model);
            }
            return RedirectToRoute("*.Error.Http404");
        }

        [AllowAnonymous]
        public ActionResult Blog()
        {
            ViewBag.Title = "Blogging Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Blog.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Git()
        {
            ViewBag.Title = "Git Service Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Git.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult IRC()
        {
            ViewBag.Title = "IRC Server Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/IRC.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Mail()
        {
            ViewBag.Title = "Mail Server Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Mail.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Mumble()
        {
            ViewBag.Title = "Mumble Server Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Mumble.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Upload()
        {
            ViewBag.Title = "Upload Service Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Upload.cshtml", model);
        }
    }
}