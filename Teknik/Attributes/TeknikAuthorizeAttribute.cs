using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Error.Controllers;
using Teknik.Utilities;
using Teknik.Areas.Users.Controllers;
using Teknik.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Areas.Users.Models;
using Teknik.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Teknik.Logging;
using Teknik.Data;
using Teknik.Security;

namespace Teknik.Attributes
{
    public enum AuthType
    {
        Basic,
        Forms
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TeknikAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private AuthType m_AuthType { get; set; }

        public TeknikAuthorizeAttribute() : this(AuthType.Forms)
        {
        }

        public TeknikAuthorizeAttribute(AuthType authType)
        {
            m_AuthType = authType;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                //if (m_AuthType == AuthType.Forms)
                //{
                //    var logger = (ILogger<Logger>)context.HttpContext.RequestServices.GetService(typeof(ILogger<Logger>));
                //    var config = (Config)context.HttpContext.RequestServices.GetService(typeof(Config));
                //    var dbContext = (TeknikEntities)context.HttpContext.RequestServices.GetService(typeof(TeknikEntities));
                //    var logoutSession = (LogoutSessionManager)context.HttpContext.RequestServices.GetService(typeof(LogoutSessionManager));

                //    var userController = new UserController(logger, config, dbContext, logoutSession);
                //    if (userController != null)
                //    {
                //        // auth failed, redirect to login page
                //        var request = context.HttpContext.Request;
                //        string redirectUrl = (request.Host != null && request.Path != null) ? request.Host + request.Path : string.Empty;

                //        context.Result = userController.Login(redirectUrl);
                //        return;
                //    }
                //}
            }
        }
    }
}
