using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Areas.Error.Controllers;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRouter _router;

        public ErrorHandlerMiddleware(RequestDelegate next, IRouter router)
        {
            _next = next;
            _router = router;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<Logger> logger, Config config, TeknikEntities dbContext)
        {
            var statusCodeFeature = new StatusCodePagesFeature();
            httpContext.Features.Set<IStatusCodePagesFeature>(statusCodeFeature);

            Exception exception = null;
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (!statusCodeFeature.Enabled)
            {
                // Check if the feature is still available because other middleware (such as a web API written in MVC) could
                // have disabled the feature to prevent HTML status code responses from showing up to an API client.
                return;
            }

            // Do nothing if a response body has already been provided or not 404 response
            if (httpContext.Response.HasStarted)
            {
                return;
            }

            // Detect if there is a response code or exception occured
            if ((httpContext.Response.StatusCode >= 400 && httpContext.Response.StatusCode <= 600) || exception != null)
            {
                RouteData routeData = new RouteData();
                routeData.DataTokens.Add("area", "Error");
                routeData.Values.Add("controller", "Error");
                routeData.Routers.Add(_router);

                var context = new ControllerContext();
                context.HttpContext = httpContext;
                context.RouteData = routeData;
                context.ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor();

                ErrorController errorController = new ErrorController(logger, config, dbContext);
                errorController.ControllerContext = context;

                if (httpContext.Response.StatusCode == 500 || exception != null)
                {
                    await errorController.Http500(exception).ExecuteResultAsync(context);
                }
                else
                {
                    await errorController.HttpError(httpContext.Response.StatusCode).ExecuteResultAsync(context);
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SetupErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder, Config config)
        {
            var routes = new RouteBuilder(builder)
            {
                DefaultHandler = builder.ApplicationServices.GetRequiredService<MvcRouteHandler>(),
            };
            routes.BuildRoutes(config);

            return builder.UseMiddleware<ErrorHandlerMiddleware>(routes.Build());
        }
    }
}
