using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Middleware
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
                    allowedDomain = host;
                }

                var csp = string.Format(
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
                    "frame-ancestors {0};",
                    allowedDomain,
                    httpContext.Items[Constants.NONCE_KEY]);

                if (!httpContext.Response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    httpContext.Response.Headers.Add("Content-Security-Policy", csp);
                }
                // and once again for IE
                if (!httpContext.Response.Headers.ContainsKey("X-Content-Security-Policy"))
                {
                    httpContext.Response.Headers.Add("X-Content-Security-Policy", csp);
                }
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
