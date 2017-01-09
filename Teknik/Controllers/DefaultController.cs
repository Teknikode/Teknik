using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Teknik.Areas.Error.Controllers;
using Teknik.Areas.Error.ViewModels;
using Teknik.Configuration;

using Piwik.Tracker;
using Teknik.Filters;
using Teknik.Helpers;
using Teknik.ViewModels;

namespace Teknik.Controllers
{
    [CORSActionFilter]
    public class DefaultController : Controller
    {
        private Config _config;

        protected string Subdomain
        {
            get { return (string)Request.RequestContext.RouteData.Values["sub"]; }
        }

        protected Config Config
        {
            get
            {
                if (_config == null)
                {
                    _config = Config.Load();
                }
                return _config;
            }
        }

        public DefaultController()
        {
            ViewBag.Title = Config.Title;
            ViewBag.Description = Config.Description;

            if (Response != null)
            {
                Response.SuppressFormsAuthenticationRedirect = true;
            }
        }

        // Get the Favicon
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Favicon()
        {
            // Get favicon
            string imageFile = Server.MapPath(Constants.FAVICON_PATH);
            return File(imageFile, "image/x-icon");
        }

        // Get the Logo
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Logo()
        {
            // Get favicon
            string imageFile = Server.MapPath(Constants.LOGO_PATH);
            return File(imageFile, "image/svg+xml");
        }

        // Get the Logo
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Robots()
        {
            // Get favicon
            string file = Server.MapPath(Constants.ROBOTS_PATH);
            return File(file, "plain/text");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            var errorController = new ErrorController();
            if (errorController != null)
            {
                return errorController.Http404(new Exception("Page Not Found"));
            }
            return null;
        }

        protected ActionResult GenerateActionResult(object json)
        {
            return GenerateActionResult(json, View());
        }

        protected ActionResult GenerateActionResult(object json, ActionResult result)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(json);
            }
            return result;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
                return false;

            return true;
        }
    }
}