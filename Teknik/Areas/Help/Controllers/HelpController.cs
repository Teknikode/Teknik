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
        public ActionResult Topic(string topic)
        {
            ViewBag.Title = topic + " Help - " + Config.Title;
            HelpViewModel model = new HelpViewModel();

            string view = string.Empty;
            switch(topic.ToLower())
            {
                case "api":
                    view = "~/Areas/Help/Views/Help/Api.cshtml";
                    break;
                case "blog":
                    view = "~/Areas/Help/Views/Help/Blog.cshtml";
                    break;
                case "git":
                    view = "~/Areas/Help/Views/Help/Git.cshtml";
                    break;
                case "irc":
                    view = "~/Areas/Help/Views/Help/Irc.cshtml";
                    break;
                case "mail":
                    view = "~/Areas/Help/Views/Help/Mail.cshtml";
                    break;
                case "mumble":
                    view = "~/Areas/Help/Views/Help/Mumble.cshtml";
                    break;
                case "upload":
                    view = "~/Areas/Help/Views/Help/Upload.cshtml";
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(view))
            {
                return View(view, model);
            }
            return RedirectToRoute("*.Error.Http404");
        }
    }
}