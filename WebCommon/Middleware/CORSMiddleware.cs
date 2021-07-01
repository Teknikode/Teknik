using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Utilities.Routing;

namespace Teknik.WebCommon.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CORSMiddleware
    {
        private readonly RequestDelegate _next;

        public CORSMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext httpContext, Config config)
        {
            // Allow this domain, or everything if local
            string origin = (httpContext.Request.IsLocal()) ? "*" : httpContext.Request.Headers["Origin"].ToString();

            // Is the referrer set to the CDN and we are using a CDN?
            if (config.UseCdn && !string.IsNullOrEmpty(config.CdnHost))
            {
                try
                {
                    string host = httpContext.Request.Headers["Host"];
                    Uri uri = new Uri(config.CdnHost);
                    if (host == uri.Host)
                        origin = host;
                }
                catch { }
            }

            string domain = (string.IsNullOrEmpty(origin)) ? string.Empty : origin.GetDomain();

            if (string.IsNullOrEmpty(origin))
            {
                string host = httpContext.Request.Headers["Host"];
                string sub = host.GetSubdomain();
                origin = (string.IsNullOrEmpty(sub)) ? config.Host : sub + "." + config.Host;
            }
            else
            {
                if (domain != config.Host)
                {
                    string sub = origin.GetSubdomain();
                    origin = (string.IsNullOrEmpty(sub)) ? config.Host : sub + "." + config.Host;
                }
            }

            httpContext.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            httpContext.Response.Headers.Append("Vary", "Origin");

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CORSMiddlewareExtensions
    {
        public static IApplicationBuilder UseCORS(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CORSMiddleware>();
        }
    }
}
