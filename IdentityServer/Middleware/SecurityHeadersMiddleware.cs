using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;

namespace Teknik.IdentityServer.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, Config config)
        {
            IHeaderDictionary headers = httpContext.Response.Headers;

            // Access Control
            headers.Append("Access-Control-Allow-Credentials", "true");
            headers.Append("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
            headers.Append("Access-Control-Allow-Headers", "Authorization, Accept, Origin, Content-Type, X-Requested-With, Connection, Transfer-Encoding");

            // HSTS
            headers.Append("strict-transport-security", "max-age=31536000; includeSubdomains; preload");

            // XSS Protection
            headers.Append("X-XSS-Protection", "1; mode=block");

            // Content Type Options
            headers.Append("X-Content-Type-Options", "nosniff");

            // Referrer Policy
            headers.Append("Referrer-Policy", "no-referrer, strict-origin-when-cross-origin");

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
