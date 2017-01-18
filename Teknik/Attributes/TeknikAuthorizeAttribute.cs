using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Teknik.Areas.Error.Controllers;
using Teknik.Utilities;
using Teknik.Areas.Users.Controllers;

namespace Teknik.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class TeknikAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
            {
                // If a child action cache block is active, we need to fail immediately, even if authorization
                // would have succeeded. The reason is that there's no way to hook a callback to rerun
                // authorization before the fragment is served from the cache, so we can't guarantee that this
                // filter will be re-run on subsequent requests.
                throw new InvalidOperationException("AuthorizeAttribute cannot be used within a child action caching block.");
            }

            // Check to see if we want to skip Authentication Check
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
            || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);
            if (skipAuthorization)
                return;

            // Check the users auth
            if (base.AuthorizeCore(filterContext.HttpContext))
            {
                // ** IMPORTANT **
                // Since we're performing authorization at the action level, the authorization code runs
                // after the output caching module. In the worst case this could allow an authorized user
                // to cause the page to be cached, then an unauthorized user would later be served the
                // cached page. We work around this by telling proxies not to cache the sensitive page,
                // then we hook our custom authorization code into the caching mechanism so that we have
                // the final say on whether a page should be served from the cache.

                HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
                cachePolicy.SetProxyMaxAge(new TimeSpan(0));
                cachePolicy.AddValidationCallback(CacheValidateHandler, null /* data */);

                return;
            }
            else if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                this.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                // uh oh, they are authorized, but don't have access.   ABORT ABORT ABORT
                HandleInvalidAuthRequest(filterContext);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // auth failed, redirect to login page
            var request = filterContext.HttpContext.Request;
            string redirectUrl = (request.Url != null) ? filterContext.HttpContext.Request.Url.AbsoluteUri.ToString() : string.Empty;

            var userController = new UserController();
            if (userController != null)
            {
                filterContext.Result = userController.Login(redirectUrl);
                return;
            }
            filterContext.Result = new HttpUnauthorizedResult();
        }

        protected void HandleInvalidAuthRequest(AuthorizationContext filterContext)
        {
            // auth failed, redirect to login page
            var request = filterContext.HttpContext.Request;
            string redirectUrl = (request.Url != null) ? filterContext.HttpContext.Request.Url.AbsoluteUri.ToString() : string.Empty;

            var errorController = new ErrorController();
            if (errorController != null)
            {
                filterContext.Result = errorController.Http403(new Exception("Not Authorized"));
                return;
            }
            filterContext.Result = new HttpUnauthorizedResult();
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = base.OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}