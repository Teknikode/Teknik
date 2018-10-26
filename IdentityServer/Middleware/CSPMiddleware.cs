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

                var csp = "default-src 'self';" +
                         "img-src * 'self' data: https:;" +
                         $"style-src 'self' {allowedDomain};" +
                         $"font-src 'self' {allowedDomain};" +
                         $"script-src 'self' 'unsafe-inline' {allowedDomain};";

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
