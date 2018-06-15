using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Utilities;

namespace Teknik.Middleware
{
    public class SetupHttpContextMiddleware
    {
        private readonly RequestDelegate _next;

        public SetupHttpContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, Config config)
        {
            // Setup the HTTP Context for everything else

            // Generate the NONCE used for this request
            string nonce = Convert.ToBase64String(Encoding.UTF8.GetBytes(StringHelper.RandomString(24)));
            httpContext.Items[Constants.NONCE_KEY] = nonce;

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SetupHttpContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpContextSetup(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SetupHttpContextMiddleware>();
        }
    }
}
