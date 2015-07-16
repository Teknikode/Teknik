using System.Web.Mvc;
using System.Web.Routing;

namespace Teknik
{
    public static class SubdomainRouteExtension
    {
        public static void MapSubdomainRoute(this RouteCollection routes, string name, string url, object defaults = null, object constraints = null)
        {
            routes.Add(name, new SubdomainRoute(url)
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            });
        }
    }
}