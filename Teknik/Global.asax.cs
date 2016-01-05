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
using Teknik.Migrations;
using System.Data.Entity.Migrations;
using Teknik.Areas.Profile.Models;
using System.ComponentModel;
using Teknik.Areas.Error.Controllers;
using System.Web.Helpers;
using System.Diagnostics;

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

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            
            var stopwatch = new Stopwatch();
            HttpContext.Current.Items["Stopwatch"] = stopwatch;
            stopwatch.Start();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;

            Stopwatch stopwatch = (Stopwatch)context.Items["Stopwatch"];
            stopwatch.Stop();

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0} seconds", ts.TotalSeconds);

            context.Response.AppendHeader("GenerationTime", elapsedTime);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            if (FormsAuthentication.CookiesSupported == true)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    //let us take out the username now                
                    string username = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value).Name;
                    List<string> roles = new List<string>();

                    using (TeknikEntities entities = new TeknikEntities())
                    {
                        User user = entities.Users.Include("Groups").Include("Groups.Roles").SingleOrDefault(u => u.Username == username);

                        if (user != null)
                        {
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
                    }

                    //Let us set the Pricipal with our user specific details
                    HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(
                        new System.Security.Principal.GenericIdentity(username, "Forms"), roles.ToArray());
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

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
            if (IsAjaxRequest())
            {
                string jsonResult = string.Empty;
                if (httpException == null)
                {
                    jsonResult = Json.Encode(new { error = new { type = "Exception", message = exception.Message } });
                }
                else
                {
                    jsonResult = Json.Encode(new { error = new { type = "Http", statuscode = httpException.GetHttpCode(), message = exception.Message } });
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

        //This method checks if we have an AJAX request or not
        private bool IsAjaxRequest()
        {
            //The easy way
            bool isAjaxRequest = (Request["X-Requested-With"] == "XMLHttpRequest")
            || ((Request.Headers != null)
            && (Request.Headers["X-Requested-With"] == "XMLHttpRequest"));

            //If we are not sure that we have an AJAX request or that we have to return JSON 
            //we fall back to Reflection
            if (!isAjaxRequest)
            {
                try
                {
                    //The controller and action
                    string controllerName = Request.RequestContext.
                                            RouteData.Values["controller"].ToString();
                    string actionName = Request.RequestContext.
                                        RouteData.Values["action"].ToString();

                    //We create a controller instance
                    DefaultControllerFactory controllerFactory = new DefaultControllerFactory();
                    Controller controller = controllerFactory.CreateController(
                    Request.RequestContext, controllerName) as Controller;

                    //We get the controller actions
                    ReflectedControllerDescriptor controllerDescriptor =
                    new ReflectedControllerDescriptor(controller.GetType());
                    ActionDescriptor[] controllerActions =
                    controllerDescriptor.GetCanonicalActions();

                    //We search for our action
                    foreach (ReflectedActionDescriptor actionDescriptor in controllerActions)
                    {
                        if (actionDescriptor.ActionName.ToUpper().Equals(actionName.ToUpper()))
                        {
                            //If the action returns JsonResult then we have an AJAX request
                            if (actionDescriptor.MethodInfo.ReturnType
                            .Equals(typeof(JsonResult)))
                                return true;
                        }
                    }
                }
                catch
                {

                }
            }

            return isAjaxRequest;
        }
    }
}
