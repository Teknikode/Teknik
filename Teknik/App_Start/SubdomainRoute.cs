using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Teknik
{
    public class SubdomainRoute : Route
    {
        public string subDomain { get; set; }

        public SubdomainRoute(string subdomain, string url, IRouteHandler handler)
        : base(url, handler)
        {
            this.subDomain = subdomain;
        }
        public SubdomainRoute(string subdomain, string url, RouteValueDictionary defaults, IRouteHandler handler)
        : base(url, defaults, handler)
        {
            this.subDomain = subdomain;
        }

        public SubdomainRoute(string subdomain, string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler handler)
        : base(url, defaults, constraints, handler)
        {
            this.subDomain = subdomain;
        }

        public SubdomainRoute(string subdomain, string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler handler)
        : base(url, defaults, constraints, dataTokens, handler)
        {
            this.subDomain = subdomain;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext);
            if (routeData == null) return null; // Only look at the subdomain if this route matches in the first place.
            string subdomain = httpContext.Request.QueryString["sub"]; // A subdomain specified as a query parameter takes precedence over the hostname.
            if (subdomain == null)
            {
                string host = httpContext.Request.Headers["Host"];
                if (host.Split('.').Count() > 2)
                {
                    int index = host.IndexOf('.');
                    if (index >= 0)
                        subdomain = host.Substring(0, index);
                }
                else
                {
                    subdomain = string.Empty;
                }
            }
            else
            {
                if (routeData.Values["sub"] == null)
                {
                    routeData.Values["sub"] = subdomain;
                }
                else
                {
                    subdomain = routeData.Values["sub"].ToString();
                }
            }

            //routeData.Values["sub"] = subdomain;
            if (subDomain == "*" || subDomain == subdomain)
            {
                return routeData;
            }
            return null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            object subdomainParam = requestContext.HttpContext.Request.QueryString["sub"];
            if (subdomainParam != null && values["sub"] == null)
                values["sub"] = subdomainParam;
            return base.GetVirtualPath(requestContext, values); // we now have the route based on subdomain
        }
    }
}