using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Teknik.Areas.Error.Controllers;
using Teknik.Utilities;
using Teknik.Areas.Users.Controllers;
using Teknik.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Areas.Users.Models;
using Teknik.Configuration;

namespace Teknik.Attributes
{
    public enum AuthType
    {
        Basic,
        Forms
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TeknikAuthorizeAttribute : AuthorizeAttribute
    {
        private AuthType m_AuthType { get; set; }

        public TeknikAuthorizeAttribute() : this(AuthType.Forms)
        {
        }

        public TeknikAuthorizeAttribute(AuthType authType)
        {
            m_AuthType = authType;
        }

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
            if (AuthorizeCore(filterContext.HttpContext))
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
                HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                // uh oh, they are authorized, but don't have access.   ABORT ABORT ABORT
                HandleInvalidAuthRequest(filterContext);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            switch (m_AuthType)
            {
                case AuthType.Basic:
                    #region Basic Authentication
                    // Let's see if they have an auth token
                    if (httpContext.Request != null)
                    {
                        if (httpContext.Request.Headers.HasKeys())
                        {
                            string auth = httpContext.Request.Headers["Authorization"];
                            if (!string.IsNullOrEmpty(auth))
                            {
                                string[] parts = auth.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                string type = string.Empty;
                                string value = string.Empty;
                                if (parts.Length > 0)
                                {
                                    type = parts[0].ToLower();
                                }
                                if (parts.Length > 1)
                                {
                                    value = parts[1];
                                }

                                using (TeknikEntities entities = new TeknikEntities())
                                {
                                    // Get the user information based on the auth type
                                    switch (type)
                                    {
                                        case "basic":
                                            KeyValuePair<string, string> authCreds = StringHelper.ParseBasicAuthHeader(value);
                                            bool validToken = UserHelper.UserTokenCorrect(entities, authCreds.Key, authCreds.Value);

                                            if (validToken)
                                            {
                                                User user = UserHelper.GetUserFromToken(entities, authCreds.Key, authCreds.Value);
                                                return UserHelper.UserHasRoles(entities, user, Roles);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    return false;
                case AuthType.Forms:
                    return base.AuthorizeCore(httpContext);
                default:
                    return base.AuthorizeCore(httpContext);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            ActionResult result = new HttpUnauthorizedResult();
            switch (m_AuthType)
            {
                case AuthType.Basic:
                    Config config = Config.Load();
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", String.Format("Basic realm=\"{0}\"", config.Title));
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
                    result = new JsonResult() { Data = new { error = new { type = 401, message = "Unauthorized" } }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    break;
                case AuthType.Forms:
                    var userController = new UserController();
                    if (userController != null)
                    {
                        // auth failed, redirect to login page
                        var request = filterContext.HttpContext.Request;
                        string redirectUrl = (request.Url != null) ? filterContext.HttpContext.Request.Url.AbsoluteUri.ToString() : string.Empty;

                        result = userController.Login(redirectUrl);
                    }
                    break;
                default:
                    break;
            }

            filterContext.Result = result;
        }

        protected void HandleInvalidAuthRequest(AuthorizationContext filterContext)
        {
            ActionResult result = new HttpUnauthorizedResult();
            switch (m_AuthType)
            {
                case AuthType.Basic:
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.StatusCode = 403;
                    filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                    filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
                    result = new JsonResult() { Data = new { error = new { type = 403, message = "Not Authorized" } }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    break;
                case AuthType.Forms:
                    var errorController = new ErrorController();
                    if (errorController != null)
                    {
                        result = errorController.Http403(new Exception("Not Authorized"));
                    }
                    break;
                default:
                    break;
            }

            filterContext.Result = result;
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = base.OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}