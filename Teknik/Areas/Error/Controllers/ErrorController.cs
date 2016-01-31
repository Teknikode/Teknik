using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Error.ViewModels;
using Teknik.Controllers;

namespace Teknik.Areas.Error.Controllers
{
    public class ErrorController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Exception(Exception exception)
        {
            ViewBag.Title = "Exception - " + Config.Title;

            if (Response != null)
                Response.StatusCode = 200;

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult General(Exception exception)
        {
            ViewBag.Title = "Http Exception - " + Config.Title;

            ErrorViewModel model = new ErrorViewModel();
            model.Description = exception.Message;
            model.Exception = exception;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Http403(Exception exception)
        {
            ViewBag.Title = "403 - " + Config.Title;
            ViewBag.Message = "Access Denied";

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Http404(Exception exception)
        {
            ViewBag.Title = "404 - " + Config.Title;
            ViewBag.Message = "Uh Oh, can't find it!";

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Http500(Exception exception)
        {
            ViewBag.Title = "500 - " + Config.Title;
            ViewBag.Message = "Something Borked";

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View(model);
        }
    }
}