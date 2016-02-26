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

namespace Teknik.Controllers
{
    [TrackingFilter]
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
            ViewBag.Message = Config.Description;

            if (Response != null)
            {
                Response.SuppressFormsAuthenticationRedirect = true;
            }
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