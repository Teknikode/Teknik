using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace Teknik
{
    public static class UrlExtensions
    {
        public static string SubRouteUrl(this UrlHelper url, string sub, string routeName)
        {
            return url.SubRouteUrl(sub, routeName, null);
        }

        /// <summary>
        /// Generates a full URL given the specified sub domain and route name
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sub"></param>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string SubRouteUrl(this UrlHelper url, string sub, string routeName, object routeValues)
        {
            string host = url.RequestContext.HttpContext.Request.Url.Authority;
            
            string domain = host;
            string rightUrl = string.Empty;

            // get current subdomain
            string curSub = host.GetSubdomain();
            var split = host.Split('.'); // split the host by '.'
            if (split.Count() > 2)
            {
                int index = host.IndexOf('.') + 1;
                if (index >= 0 && index < host.Length)
                    domain = host.Substring(index, (host.Length - index));
            }

            // Grab the sub from parameters if it exists
            string subParam = url.RequestContext.HttpContext.Request.QueryString["sub"]; // A subdomain specified as a query parameter takes precedence over the hostname.

            // If the param is not being used, we will use the curSub
            if (string.IsNullOrEmpty(subParam))
            {
                string firstSub = (curSub == "dev") ? "dev" : sub;
                if (!string.IsNullOrEmpty(firstSub))
                {
                    routeName = firstSub + "." + routeName;
                    domain = firstSub + "." + domain;
                }
            }
            else
            {
                string desiredSub = (subParam == "dev") ? "dev" : sub;
                routeName = desiredSub + "." + routeName;
                domain = host;
            }

            try
            {
                rightUrl = url.RouteUrl(routeName, routeValues);
            }
            catch (ArgumentException)
            {

            }

            string absoluteAction = string.Format("{0}://{1}{2}", url.RequestContext.HttpContext.Request.Url.Scheme, domain, rightUrl);

            if (!string.IsNullOrEmpty(subParam) && subParam != "dev")
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
    }
}