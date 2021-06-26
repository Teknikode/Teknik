using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Utilities.Routing;

namespace Teknik.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CSPMiddleware
    {
        private readonly RequestDelegate _next;

        public CSPMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, Config config)
        {
            if (!httpContext.Request.IsLocal())
            {
                // Default to nothing allowed
                string allowedDomain = "'none'";

                // Allow this domain
                string host = httpContext.Request.Headers["Host"];

                if (!string.IsNullOrEmpty(host))
                {
                    string domain = host.GetDomain();

                    allowedDomain = string.Format("*.{0} {0}", domain);
                }

                // If a CDN is enabled, then add the cdn host
                if (config.UseCdn)
                {
                    allowedDomain += " " + config.CdnHost;
                }                

                httpContext.Response.Headers.Append("Content-Security-Policy", string.Format(
                    "default-src 'none'; " +
                    "script-src blob: 'unsafe-eval' 'nonce-{1}' {0}; " +
                    "style-src 'unsafe-inline' {0}; " +
                    "img-src data: *; " +
                    "font-src data: {0}; " +
                    "connect-src wss: blob: data: {0}; " +
                    "media-src *; " +
                    "worker-src blob: mediastream: {0}; " +
                    "form-action {0}; " +
                    "base-uri {0}; " +
                    "frame-ancestors {0}; " +
                    "object-src {0};",
                    allowedDomain, 
                    httpContext.Items[Constants.NONCE_KEY]));
            }

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CSPMiddlewareExtensions
    {
        public static IApplicationBuilder UseCSP(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CSPMiddleware>();
        }
    }
}
