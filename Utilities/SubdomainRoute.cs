using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Routing;
using Teknik.Utilities;

namespace Teknik.Utilities
{
    public class SubdomainRoute : Route
    {
        private readonly IRouter _target;

        public List<string> Subdomains { get; set; }

        public List<string> Domains { get; set; }

        public SubdomainRoute(List<string> subdomains, List<string> domains, IRouter router, string routeTemplate, IInlineConstraintResolver inlineConstraintResolver)
        : base(router, routeTemplate, inlineConstraintResolver)
        {
            this.Subdomains = subdomains;
            this.Domains = domains;
            this._target = router;
        }

        public SubdomainRoute(List<string> subdomains, List<string> domains, IRouter router, string routeTemplate, RouteValueDictionary defaults, IDictionary<string, object> constraints, RouteValueDictionary dataTokens, IInlineConstraintResolver inlineConstraintResolver)
        : base(router, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
            this.Subdomains = subdomains;
            this.Domains = domains;
            this._target = router;
        }

        public SubdomainRoute(List<string> subdomains, List<string> domains, IRouter router, string routeName, string routeTemplate, RouteValueDictionary defaults, IDictionary<string, object> constraints, RouteValueDictionary dataTokens, IInlineConstraintResolver inlineConstraintResolver)
        : base(router, routeName, routeTemplate, defaults, constraints, dataTokens, inlineConstraintResolver)
        {
            this.Subdomains = subdomains;
            this.Domains = domains;
            this._target = router;
        }

        protected override Task OnRouteMatched(RouteContext context)
        {
            // Only look at the subdomain if there is any route data.
            if (context.RouteData != null)
            { 
                string subdomain = context.HttpContext.Request.Query["sub"]; // A subdomain specified as a query parameter takes precedence over the hostname.
                string host = context.HttpContext.Request.Headers["Host"];

                if (host != null)
                {
                    string domain = host.GetDomain();
                    string curSub = host.GetSubdomain();

                    // special consideration for 'dev' subdomain
                    if (subdomain == null || subdomain == "dev")
                    {
                        if (!string.IsNullOrEmpty(curSub) && curSub == "dev")
                        {
                            // if we are on dev, and the param is dev or empty, we need to initialize it to 'www'
                            subdomain = "www";
                        }
                    }

                    if (subdomain == null)
                    {
                        subdomain = curSub;
                    }
                    else
                    {
                        if (context.RouteData.Values["sub"] == null)
                        {
                            context.RouteData.Values["sub"] = subdomain;
                        }
                        else
                        {
                            subdomain = context.RouteData.Values["sub"].ToString();
                        }
                    }

                    // Check if this route is valid for the current domain
                    if (context.HttpContext.Request.IsLocal() || Domains.Contains(domain))
                    {
                        // Check if this route is valid for the current subdomain ('*' means any subdomain is valid)
                        if (Subdomains.Contains("*") || Subdomains.Contains(subdomain))
                        {
                            context.RouteData.Values["sub"] = subdomain;
                            context.RouteData.Routers.Add(_target);
                            return _target.RouteAsync(context);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        public override VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            object subdomainParam = context.HttpContext.Request.Query["sub"];
            if (subdomainParam != null && context.Values["sub"] == null)
                context.Values["sub"] = subdomainParam;
            return base.GetVirtualPath(context); // we now have the route based on subdomain
        }
    }
}