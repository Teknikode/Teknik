using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Teknik.Configuration;
using Teknik.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Teknik.Middleware
{
    public class IdentityServerUrlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRouter _router;

        public IdentityServerUrlMiddleware(RequestDelegate next, IRouter router)
        {
            _next = next;
            _router = router;
        }

        public async Task Invoke(HttpContext httpContext, Config config)
        {
            RouteData routeData = new RouteData();
            routeData.Routers.Add(_router);

            var context = new ActionContext(httpContext, routeData, new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            UrlHelper urlHelper = new UrlHelper(context);

            string baseUrl = urlHelper.SubRouteUrl("auth", "Auth.IdentityServer");

            string curSub = baseUrl.GetSubdomain();
            //if (!string.IsNullOrEmpty(curSub) && curSub != "dev")

            httpContext.SetIdentityServerOrigin(baseUrl);
            httpContext.SetIdentityServerBasePath(httpContext.Request.PathBase.Value.TrimEnd('/'));

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class IdentityServerUrlMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdentityServerUrl(this IApplicationBuilder builder, Config config)
        {
            var routes = new RouteBuilder(builder)
            {
                DefaultHandler = builder.ApplicationServices.GetRequiredService<MvcRouteHandler>(),
            };
            routes.BuildRoutes(config);

            return builder.UseMiddleware<IdentityServerUrlMiddleware>(routes.Build());
        }
    }
}
