using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Teknik.Models;
using System.Data.Entity;
using System.Web.Security;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Error.Controllers;
using System.Web.Helpers;
using System.Diagnostics;
using Teknik.Utilities;
using System.Text;
using Teknik.Areas.Users.Utility;

namespace Teknik
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new CustomRazorViewEngine());

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TeknikEntities, Migrations.Configuration>());

            AreaRegistration.RegisterAllAreas();

            AntiForgeryConfig.RequireSsl = true;

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Start the generation time stopwatcher
            var stopwatch = new Stopwatch();
            HttpContext.Current.Items["Stopwatch"] = stopwatch;
            stopwatch.Start();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;

            // Set the generation time in the header
            Stopwatch stopwatch = (Stopwatch)context.Items["Stopwatch"];
            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0} seconds", ts.TotalSeconds);

            context.Response.AppendHeader("GenerationTime", elapsedTime);

            // Allow this domain, or everything if local
            string origin = (Request.IsLocal) ? "*" : context.Request.Headers.Get("Origin");
            if (!string.IsNullOrEmpty(origin))
            {
                context.Response.AppendHeader("Access-Control-Allow-Origin", origin);
            }
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            // We support both Auth Tokens and Cookie Authentication

            // Username and Roles for the current user
            string username = string.Empty;
            List<string> roles = new List<string>();
            
            bool hasAuthToken = false;
            if (Request != null)
            {
                if (Request.Headers.HasKeys())
                {
                    string auth = Request.Headers["Authorization"];
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
            }

            if (FormsAuthentication.CookiesSupported == true && !hasAuthToken)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    //let us take out the username now                
                    username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                }
            }

            // Create the new user if we found one from the supplied auth info
            if (!string.IsNullOrEmpty(username))
            {
                using (TeknikEntities entities = new TeknikEntities())
                {
                    User user = UserHelper.GetUser(entities, username);

                    // Grab all their roles
                    foreach (Group grp in user.Groups)
                    {
                        foreach (Role role in grp.Roles)
                        {
                            if (!roles.Contains(role.Name))
                            {
                                roles.Add(role.Name);
                            }
                        }
                    }
                }

                HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                    new System.Security.Principal.GenericIdentity(username, "Forms"), roles.ToArray());
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Get the last exception
            Exception exception = Server.GetLastError();
            
            // Clear the response
            Response.Clear();

            HttpException httpException = exception as HttpException;

            RouteData routeData = new RouteData();
            routeData.DataTokens.Add("namespaces", new[] { typeof(ErrorController).Namespace });
            routeData.DataTokens.Add("area", "Error");
            routeData.Values.Add("controller", "Error");
            
            if (httpException == null)
            {
                routeData.Values.Add("action", "Exception");
            }
            else //It's an Http Exception, Let's handle it.
            {
                switch (httpException.GetHttpCode())
                {
                    case 401:
                        // Unauthorized.
                        routeData.Values.Add("action", "Http401");
                        break;
                    case 403:
                        // Forbidden.
                        routeData.Values.Add("action", "Http403");
                        break;
                    case 404:
                        // Page not found.
                        routeData.Values.Add("action", "Http404");
                        break;
                    case 500:
                        // Server error.
                        routeData.Values.Add("action", "Http500");
                        break;

                    // Here you can handle Views to other error codes.
                    // I choose a General error template  
                    default:
                        routeData.Values.Add("action", "General");
                        break;
                }
            }

            // Pass exception details to the target error View.
            routeData.Values.Add("exception", exception);

            // Clear the error on server.
            Server.ClearError();

            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;

            // If it is an Ajax request, we should respond with Json data, otherwise redirect
            if (new HttpRequestWrapper(Request).IsAjaxRequest())
            {
                string jsonResult = string.Empty;
                if (httpException == null)
                {
                    jsonResult = Json.Encode(new { error = new { type = "Exception", message = exception.GetFullMessage(true) } });
                }
                else
                {
                    jsonResult = Json.Encode(new { error = new { type = "Http", statuscode = httpException.GetHttpCode(), message = exception.GetFullMessage(true) } });
                }
                Response.Write(jsonResult);
            }
            else
            {
                // Call target Controller and pass the routeData.
                IController errorController = new ErrorController();
                errorController.Execute(new RequestContext(
                     new HttpContextWrapper(Context), routeData));
            }
        }
    }
}
