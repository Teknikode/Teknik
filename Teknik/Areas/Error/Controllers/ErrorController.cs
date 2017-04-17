using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Error.ViewModels;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Logging;
using Teknik.Attributes;

namespace Teknik.Areas.Error.Controllers
{
    [TeknikAuthorize]
    public class ErrorController : DefaultController
    {
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Exception(Exception exception)
        {
            ViewBag.Title = "Exception - " + Config.Title;
            ViewBag.Description = "Just a boring 'ol exception. Nothing to see here, move along.";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }

            string errorMessage = "General Exception";
            if (Request != null && Request.Url != null)
            {
                errorMessage += " on page: " + Request.Url.AbsoluteUri;
            }

            Logger.WriteEntry(LogLevel.Error, errorMessage, exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Exception.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult General(Exception exception)
        {
            ViewBag.Title = "Http Exception - " + Config.Title;
            ViewBag.Description = "There has been a Http exception.  Run!";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }

            string errorMessage = "General HTTP Exception";
            if (Request != null && Request.Url != null)
            {
                errorMessage += " on page: " + Request.Url.AbsoluteUri;
            }

            Logger.WriteEntry(LogLevel.Error, errorMessage, exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Description = exception.Message;
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/General.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Http401(Exception exception)
        {
            ViewBag.Title = "401 - " + Config.Title;
            ViewBag.Description = "Unauthorized";

            if (Response != null)
            {
                Response.StatusCode = 401;
                Response.TrySkipIisCustomErrors = true;
            }

            string errorMessage = "Unauthorized";
            if (Request != null && Request.Url != null)
            {
                errorMessage += " for page: " + Request.Url.AbsoluteUri;
            }

            Logger.WriteEntry(LogLevel.Error, errorMessage, exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http401.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Http403(Exception exception)
        {
            ViewBag.Title = "403 - " + Config.Title;
            ViewBag.Description = "Access Denied";

            if (Response != null)
            {
                Response.StatusCode = 403;
                Response.TrySkipIisCustomErrors = true;
            }

            string errorMessage = "Access Denied";
            if (Request != null && Request.Url != null)
            {
                errorMessage += " on page: " + Request.Url.AbsoluteUri;
            }

            Logger.WriteEntry(LogLevel.Error, errorMessage, exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http403.cshtml", model);
        }
        
        [AllowAnonymous]
        public ActionResult Http404(Exception exception)
        {
            ViewBag.Title = "404 - " + Config.Title;
            ViewBag.Description = "Uh Oh, can't find it!";

            if (Response != null)
            {
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
            }

            string errorMessage = "Page Not Found";

            if (Request != null)
            {
                if (Request.Url != null)
                {
                    errorMessage += " for page: " + Request.Url.AbsoluteUri;
                }

                errorMessage += " - using Method: " + Request.HttpMethod;
            }

            Logger.WriteEntry(LogLevel.Warning, errorMessage, exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http404.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Http500(Exception exception)
        {
            ViewBag.Title = "500 - " + Config.Title;
            ViewBag.Description = "Something Borked";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }

            string errorMessage = "Server Error";
            if (Request != null && Request.Url != null)
            {
                errorMessage += " on page: " + Request.Url.AbsoluteUri;
            }

            Logger.WriteEntry(LogLevel.Error, errorMessage, exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http500.cshtml", model);
        }

        private string GetIPAddress()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}