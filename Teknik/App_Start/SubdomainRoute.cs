using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Teknik
{
    class SubdomainRoute : Route
    {
        public SubdomainRoute(string url) : base(url, new MvcRouteHandler()) { }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext);
            if (routeData == null) return null; // Only look at the subdomain if this route matches in the first place.
            string subdomain = httpContext.Request.Params["subdomain"]; // A subdomain specified as a query parameter takes precedence over the hostname.
            if (subdomain == null)
            {
                string host = httpContext.Request.Headers["Host"];
                int index = host.IndexOf('.');
                if (index >= 0)
                    subdomain = host.Substring(0, index);
            }
            if (subdomain != null)
                routeData.Values["subdomain"] = subdomain;
            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            object subdomainParam = requestContext.HttpContext.Request.Params["subdomain"];
            if (subdomainParam != null)
                values["subdomain"] = subdomainParam;
            return base.GetVirtualPath(requestContext, values);
        }
    }
}