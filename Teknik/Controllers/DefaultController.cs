using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teknik.Controllers
{
    public class DefaultController : Controller
    {
        private Config _config;

        protected string Subdomain
        {
            get { return (string)Request.RequestContext.RouteData.Values["subdomain"]; }
        }

        protected Config Config
        {
            get
            {
                if (_config == null)
                {
                    string configContents = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/App_Data/Config.json"));
                    _config = Config.Deserialize(configContents);
                    ViewBag.Config = _config;
                }
                return _config;
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