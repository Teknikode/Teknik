using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class PerformanceMonitorMiddleware
    {
        private readonly RequestDelegate _next;

        public PerformanceMonitorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, Config config)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            httpContext.Response.OnStarting(state =>
            {
                var context = (HttpContext)state;

                timer.Stop();

                double ms = (double)timer.ElapsedMilliseconds;
                string result = string.Format("{0:F0}", ms);

                if (!httpContext.Response.Headers.IsReadOnly)
                    httpContext.Response.Headers.Add("GenerationTime", result);

                return Task.CompletedTask;
            }, httpContext);

            await _next(httpContext);

            // Don't interfere with non-HTML responses 
            if (httpContext.Response.ContentType != null && httpContext.Response.ContentType.StartsWith("text/html") && httpContext.Response.StatusCode == 200 && !httpContext.Request.IsAjaxRequest())
            {
                double ms = (double)timer.ElapsedMilliseconds;
                string result = string.Format("{0:F0}", ms);

                await httpContext.Response.WriteAsync(
                        "<script nonce=\"" + httpContext.Items[Constants.NONCE_KEY] + "\">" +
                            "var pageGenerationTime = '" + result + "';" +
                            "pageloadStopTimer();" +
                        "</script >");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PerformanceMonitorMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMonitor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMonitorMiddleware>();
        }
    }
}
