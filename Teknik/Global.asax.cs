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
using Teknik.Security;

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
            try
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
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Server cannot append header after HTTP headers have been sent"))
                {
                    // Just log it
                    Logging.Logger.WriteEntry(Logging.LogLevel.Warning, "Error in Application_EndRequest", ex);
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = null;
            try
            {
                // Get the last exception
                exception = Server.GetLastError();

                // Clear the response
                Response.Clear();

                HttpException httpException = exception as HttpException;

                RouteData routeData = new RouteData();
                routeData.DataTokens.Add("namespaces", new[] { typeof(ErrorController).Namespace });
                routeData.DataTokens.Add("area", "Error");
                routeData.Values.Add("controller", "Error");
                routeData.Values.Add("scheme", "https");

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
            catch (Exception ex)
            {
                // Unable to display error, so try to log it
                try
                {
                    Logging.Logger.WriteEntry(Logging.LogLevel.Warning, "Error in Application_Error", ex);
                    if (exception != null)
                    {
                        Logging.Logger.WriteEntry(Logging.LogLevel.Error, "Exception Thrown", exception);
                    }
                }
                catch(Exception) { }
            }
        }
    }
}
