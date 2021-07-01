using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using Teknik.WebCommon;

namespace Teknik.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IErrorController errorController)
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
                var routeData = httpContext.GetRouteData() ?? new RouteData();

                var context = new ControllerContext();
                context.HttpContext = httpContext;
                context.RouteData = routeData;
                context.ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor();

                errorController.ControllerContext = context;

                await errorController.HttpError(httpContext.Response.StatusCode, exception).ExecuteResultAsync(context);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SetupErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
