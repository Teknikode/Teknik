using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Help.ViewModels;
using Teknik.Controllers;
using Teknik.Filters;

namespace Teknik.Areas.Help.Controllers
{
    public class HelpController : DefaultController
    {
        // GET: Help/Help
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View(model);
        }

        [TrackPageView]
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

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Blog()
        {
            ViewBag.Title = "Blogging Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Blog.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Git()
        {
            ViewBag.Title = "Git Service Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Git.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult IRC()
        {
            ViewBag.Title = "IRC Server Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/IRC.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Mail()
        {
            ViewBag.Title = "Mail Server Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Mail.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Mumble()
        {
            ViewBag.Title = "Mumble Server Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Mumble.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult RSS()
        {
            ViewBag.Title = "RSS Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/RSS.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Upload()
        {
            ViewBag.Title = "Upload Service Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Upload.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Markdown()
        {
            ViewBag.Title = "Markdown Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Markdown.cshtml", model);
        }
    }
}