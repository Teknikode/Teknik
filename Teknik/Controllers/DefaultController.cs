using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Teknik.Areas.Error.Controllers;
using Teknik.Configuration;

using Teknik.Filters;
using Teknik.Security;
using Teknik.Utilities;

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
        protected virtual new TeknikPrincipal User
        {
            get { return HttpContext.User as TeknikPrincipal; }
        }

        public object ObjectFactory { get; private set; }

        public DefaultController()
        {
            ViewBag.Title = Config.Title;
            ViewBag.Description = Config.Description;

            if (Response != null)
            {
                Response.SuppressFormsAuthenticationRedirect = true;
            }
        }

        protected override void HandleUnknownAction(string actionName)
        {
            this.InvokeHttp404(HttpContext);
        }
        
        [AllowAnonymous]
        public ActionResult InvokeHttp404(HttpContextBase httpContext)
        {
            IController errorController = new ErrorController();
            var errorRoute = new RouteData();
            errorRoute.DataTokens.Add("namespaces", new[] { typeof(ErrorController).Namespace });
            errorRoute.DataTokens.Add("area", "Error");
            errorRoute.Values.Add("controller", "Error");
            errorRoute.Values.Add("action", "Http404");
            errorRoute.Values.Add("exception", null);
            errorController.Execute(new RequestContext(
                 httpContext, errorRoute));

            return new EmptyResult();
        }

        // Get the Favicon
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Favicon()
        {
            // Get favicon
            string imageFile = Server.MapPath(Constants.FAVICON_PATH);

            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
            Response.Cache.SetLastModified(System.IO.File.GetLastWriteTime(imageFile));

            return File(imageFile, "image/x-icon");
        }

        // Get the Logo
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Logo()
        {
            // Get favicon
            string imageFile = Server.MapPath(Constants.LOGO_PATH);

            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(new TimeSpan(365, 0, 0, 0));
            Response.Cache.SetLastModified(System.IO.File.GetLastWriteTime(imageFile));

            return File(imageFile, "image/svg+xml");
        }

        // Get the Robots.txt
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Robots()
        {
            // Get favicon
            string file = Server.MapPath(Constants.ROBOTS_PATH);
            return File(file, "text/plain");
        }
        
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            return InvokeHttp404(HttpContext);
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
