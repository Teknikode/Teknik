using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Teknik.Utilities
{
    public static class SubdomainRouteExtension
    {
        public static SubdomainRoute MapSubdomainRoute(this IRouteBuilder routeBuilder, string name, List<string> subDomains, List<string> domains, string template, object defaults)
        {
            return MapSubdomainRoute(routeBuilder, name, subDomains, domains, template, defaults, new { }, new { });
        }

        public static SubdomainRoute MapSubdomainRoute(this IRouteBuilder routeBuilder, string name, List<string> subDomains, List<string> domains, string template, object defaults, object constraints)
        {
            return MapSubdomainRoute(routeBuilder, name, subDomains, domains, template, defaults, constraints, new { });
        }

        public static SubdomainRoute MapSubdomainRoute(this IRouteBuilder routeBuilder, string name, List<string> subDomains, List<string> domains, string template, object defaults, object constraints, object dataTokens)
        {
            var inlineConstraintResolver = routeBuilder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();

            SubdomainRoute route = new SubdomainRoute(
                subDomains,
                domains,
                routeBuilder.DefaultHandler,
                name,
                template,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(dataTokens),
                inlineConstraintResolver);

            routeBuilder.Routes.Add(route);

            return route;
        }
    }
}