using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Teknik.Utilities
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
            string host = url.ActionContext.HttpContext.Request.Host.Value;
            
            string domain = host.GetDomain();
            string rightUrl = string.Empty;

            // get current subdomain
            string curSub = host.GetSubdomain();

            // Grab the sub from parameters if it exists
            string subParam = url.ActionContext.HttpContext.Request.Query["sub"]; // A subdomain specified as a query parameter takes precedence over the hostname unless on dev

            // If the param is not being used, we will use the curSub
            if (string.IsNullOrEmpty(subParam))
            {
                // If we are on dev and no subparam, we need to set the subparam to the specified sub
                subParam = (curSub == "dev") ? sub : string.Empty;
                string firstSub = (curSub == "dev") ? "dev" : sub;
                if (!string.IsNullOrEmpty(firstSub))
                {
                    domain = firstSub + "." + domain;
                }
            }
            else
            {
                string firstSub = (curSub == "dev") ? "dev" : curSub;
                if (!string.IsNullOrEmpty(firstSub))
                {
                    domain = firstSub + "." + domain;
                }
                else
                {
                    domain = host;
                }
            }

            try
            {
                rightUrl = url.RouteUrl(new UrlRouteContext() { RouteName = routeName, Values = routeValues });
            }
            catch (ArgumentException ex)
            {

            }

            string fullHost = string.Format("{0}://{1}", url.ActionContext.HttpContext.Request.Scheme, domain);
            
            if (!string.IsNullOrEmpty(hostOverride))
            {
                fullHost = hostOverride.TrimEnd('/');
            }

            string absoluteAction = string.Format("{0}{1}", fullHost, rightUrl);

            if (!string.IsNullOrEmpty(subParam))
            {
                absoluteAction = absoluteAction.SetUrlParameter("sub", sub);
            }
  
            return absoluteAction;
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
            string curSub = url.GetSubdomain();
            foreach (string sub in subs)
            {
                if (curSub == sub)
                {
                    return "active";
                }
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