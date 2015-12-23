using System.Web.Mvc;
using System.Web.Routing;

namespace Teknik
{
    public static class SubdomainRouteExtension
    {
        public static SubdomainRoute MapSubdomainRoute(this RouteCollection routes, string name, string subDomain, string url, object defaults)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new MvcRouteHandler());
            routes.Add(AddSubToName(subDomain, name), route);

            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this RouteCollection routes, string name, string subDomain, string url, object defaults, object constraints)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new MvcRouteHandler());
            routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this RouteCollection routes, string name, string subDomain, string area, string url, object defaults, string[] namespaces)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(new { }),
                new RouteValueDictionary(new { Area = area, Namespaces = namespaces }),
                new MvcRouteHandler());
            routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this AreaRegistrationContext context, string name, string subDomain, string url, object defaults)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(new {}),
                new RouteValueDictionary(new {Area = context.AreaName}),
                new MvcRouteHandler());

            context.Routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this AreaRegistrationContext context, string name, string subDomain, string url, object defaults, object constraints)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(new {Area = context.AreaName}),
                new MvcRouteHandler());

            context.Routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this AreaRegistrationContext context, string name, string subDomain, string url, object defaults, string[] namespaces)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(new {}),
                new RouteValueDictionary(new { Area = context.AreaName, Namespaces = namespaces }),
                new MvcRouteHandler());

            context.Routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this AreaRegistrationContext context, string name, string subDomain, string url, string area, object defaults)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(new { }),
                new RouteValueDictionary(new { Area = area }),
                new MvcRouteHandler());

            context.Routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this AreaRegistrationContext context, string name, string subDomain, string url, string area, object defaults, object constraints)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(new { Area = area }),
                new MvcRouteHandler());

            context.Routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        public static SubdomainRoute MapSubdomainRoute(this AreaRegistrationContext context, string name, string subDomain, string url, string area, object defaults, string[] namespaces)
        {
            SubdomainRoute route = new SubdomainRoute(
                subDomain,
                url,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(new { }),
                new RouteValueDictionary(new { Area = area, Namespaces = namespaces }),
                new MvcRouteHandler());

            context.Routes.Add(AddSubToName(subDomain, name), route);
            return route;
        }

        private static string AddSubToName(string sub, string name)
        {
            string newName = name;
            if (!string.IsNullOrEmpty(sub))
            {
                newName = sub + "." + name;
            }

            return newName;
        }
    }
}