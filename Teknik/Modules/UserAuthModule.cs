using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teknik.Areas.Error.Controllers;
using Teknik.Areas.Users.Utility;
using Teknik.Models;
using Teknik.Security;
using Teknik.Utilities;

namespace Teknik.Modules
{
    public class UserAuthModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += OnPostAuthenticateRequestHandlerExecute;
        }

        private void OnPostAuthenticateRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            string username = string.Empty;

            bool hasAuthToken = false;
            if (context.Request.Headers.HasKeys())
            {
                string auth = context.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(auth))
                {
                    string[] parts = auth.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
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

                                bool tokenValid = UserHelper.UserTokenCorrect(entities, authCreds.Key, authCreds.Value);
                                if (tokenValid)
                                {
                                    // it's valid, so let's update it's Last Used date
                                    UserHelper.UpdateTokenLastUsed(entities, authCreds.Key, authCreds.Value, DateTime.Now);

                                    // Set the username
                                    username = authCreds.Key;
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // Check if they have a Forms Auth cookie
            if (FormsAuthentication.CookiesSupported == true && !hasAuthToken)
            {
                if (context.Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    //let us take out the username now                
                    username = FormsAuthentication.Decrypt(context.Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                }
            }

            context.User = new TeknikPrincipal(username);

            // Check to see if we need to logout this user
            if (context.User != null && context.User.Identity.IsAuthenticated)
            {
                TeknikPrincipal user = (context.User as TeknikPrincipal);
                // Is the user banned?
                if (user?.Info.AccountStatus == AccountStatus.Banned)
                {
                    // Get cookie
                    HttpCookie authCookie = UserHelper.CreateAuthCookie(user.Identity.Name, false, context.Request.Url.Host.GetDomain(), context.Request.IsLocal);

                    // Signout
                    FormsAuthentication.SignOut();
                    context.Session?.Abandon();

                    // Destroy Cookies
                    authCookie.Expires = DateTime.Now.AddYears(-1);
                    context.Response.Cookies.Add(authCookie);

                    // Reset the context user
                    context.User = new TeknikPrincipal(string.Empty);
                }
            }
        }
    }
}
