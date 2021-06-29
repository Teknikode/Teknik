using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;

namespace Teknik.Utilities.Routing
{
    public static class UrlExtensions
    {
        public static string SubRouteUrl(this IUrlHelper url, string sub, string routeName)
        {
            return url.SubRouteUrl(sub, routeName, null, string.Empty);
        }

        public static string SubRouteUrl(this IUrlHelper url, string sub, string routeName, string hostOverride)
        {
            return url.SubRouteUrl(sub, routeName, null, hostOverride);
        }

        public static string SubRouteUrl(this IUrlHelper url, string sub, string routeName, object routeValues)
        {
            return url.SubRouteUrl(sub, routeName, routeValues, string.Empty);
        }

        /// <summary>
        /// Generates a full URL given the specified sub domain and route name
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sub"></param>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string SubRouteUrl(this IUrlHelper url, string sub, string routeName, object routeValues, string hostOverride)
        {
            var linkGen = url.ActionContext.HttpContext.RequestServices.GetService<LinkGenerator>();
            var env = url.ActionContext.HttpContext.RequestServices.GetService<IWebHostEnvironment>();

            string host = url.ActionContext.HttpContext.Request.Host.Value;
            string domain = host;

            // Generate a new domain if we aren't in development
            if (!env.IsEnvironment(Environments.Development) &&
                !string.IsNullOrEmpty(sub))
            {
                domain = sub + "." + domain.GetDomain();
            }
            string fullHost = string.Format("{0}://{1}", url.ActionContext.HttpContext.Request.Scheme, domain);

            var routeValueDict = new RouteValueDictionary(routeValues);

            // Get the endpoint mapping
            var mapping = EndpointHelper.GetEndpointMapping(routeName);
            if (mapping != null)
            {
                routeValueDict.TryAdd("area", mapping.Area);

                if (mapping.Defaults != null)
                {
                    var defaults = mapping.Defaults as JObject;
                    foreach (var defaultVal in defaults)
                    {
                        routeValueDict.TryAdd(defaultVal.Key, defaultVal.Value.ToObject<object>());
                    }
                }
            }

            var path = linkGen.GetPathByAddress(url.ActionContext.HttpContext, routeName, routeValueDict);
  
            return $"{fullHost}{path}";
        }

        public static string GetUrlParameters(this string url)
        {
            Uri uri = new Uri(url);
            var queryParts = HttpUtility.ParseQueryString(uri.Query);
            return queryParts.ToString();
        }

        public static string SetUrlParameter(this string url, string paramName, string value)
        {
            return new Uri(url).SetParameter(paramName, value).ToString();
        }

        public static Uri SetParameter(this Uri url, string paramName, string value)
        {
            var queryParts = HttpUtility.ParseQueryString(url.Query);
            queryParts[paramName] = value;
            return new Uri(url.AbsoluteUriExcludingQuery() + '?' + queryParts.ToString());
        }

        public static string AbsoluteUriExcludingQuery(this Uri url)
        {
            return url.AbsoluteUri.Split('?').FirstOrDefault() ?? String.Empty;
        }

        public static string GetSubdomain(this string host)
        {
            if (host.IndexOf(":") >= 0)
                host = host.Substring(0, host.IndexOf(":"));

            Regex tldRegex = new Regex(@"\.[a-z]{2,3}\.[a-z]{2}$");
            host = tldRegex.Replace(host, "");
            tldRegex = new Regex(@"\.[a-z]{2,4}$");
            host = tldRegex.Replace(host, "");

            if (host.Split('.').Length > 1)
                return host.Substring(0, host.IndexOf("."));
            else
                return string.Empty;
        }

        public static string GetSubdomain(this IUrlHelper url)
        {
            string host = url.ActionContext.HttpContext.Request.Host.Value;
            // Grab the sub from parameters if it exists
            string subParam = url.ActionContext.HttpContext.Request.Query["sub"]; // A subdomain specified as a query parameter takes precedence over the hostname unless on dev
            if (string.IsNullOrEmpty(subParam))
            {
                // If we are on dev and no subparam, we need to set the subparam to the specified sub
                subParam = host.GetSubdomain();
            }
            return subParam;
        }

        public static string GetDomain(this string host)
        {
            string domain = host;
            var split = host.Split('.'); // split the host by '.'
            if (split.Count() > 2)
            {
                int index = host.IndexOf('.') + 1;
                if (index >= 0 && index < host.Length)
                    domain = host.Substring(index, (host.Length - index));
            }
            return domain;
        }

        public static string GetDomain(this Uri uri)
        {
            string domain = uri.Host;
            var split = uri.Host.Split('.'); // split the host by '.'
            if (split.Count() > 2)
            {
                int index = uri.Host.IndexOf('.') + 1;
                if (index >= 0 && index < uri.Host.Length)
                    domain = uri.Host.Substring(index, (uri.Host.Length - index));
            }
            return domain;
        }

        public static string GetActive(this IUrlHelper url, params string[] subs)
        {
            return url.GetActive(null, null, subs);
        }

        public static string GetActive(this IUrlHelper url, string controller)
        {
            return url.GetActive(controller, null);
        }

        public static string GetActive(this IUrlHelper url, string controller, string action, params string[] subs)
        {
            var curSub = url.GetSubdomain();
            var curController = url.ActionContext.RouteData.Values["Controller"]?.ToString();
            var curAction = url.ActionContext.RouteData.Values["Action"]?.ToString();
            foreach (string sub in subs)
            {
                if (curSub == sub)
                {
                    if ((string.IsNullOrEmpty(controller) || curController == controller) &&
                        (string.IsNullOrEmpty(action) || curAction == action))
                        return "active";
                }
            }
            if (!subs.Any() &&
                (string.IsNullOrEmpty(controller) || curController == controller) &&
                (string.IsNullOrEmpty(action) || curAction == action))
            {
                return "active";
            }
            return string.Empty;
        }

        public static bool IsValidUrl(this string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if (result)
            {
                result = uriResult.IsWellFormedOriginalString();
            }
            return result;
        }

        public static string FullURL(this IUrlHelper helper, string virtualPath)
        {
            var url = string.Format("{0}://{1}{2}", helper.ActionContext.HttpContext.Request.Scheme, helper.ActionContext.HttpContext.Request.Host.ToUriComponent(), helper.Content(virtualPath));

            return url;
        }
    }
}