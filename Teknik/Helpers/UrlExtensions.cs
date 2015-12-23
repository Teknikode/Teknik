using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Teknik
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Generates a full URL given the specified sub domain.
        /// If the subdomain is not 'dev', the Controller will be removed
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sub"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string SubAction(this UrlHelper url, string sub, string action, string controller, object routeValues)
        {
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;
            string host = url.RequestContext.HttpContext.Request.Url.Authority;
            
            string paramSub = string.Empty;
            string domain = host;
            string rightUrl = string.Empty;

            // get current subdomain
            string curSub = string.Empty;
            var split = host.Split('.'); // split the host by '.'
            if (split.Count() > 2)
            {
                curSub = split[0];
                int index = host.IndexOf('.') + 1;
                if (index >= 0 && index < host.Length)
                    domain = host.Substring(index, (host.Length - index));
            }

            // Grab the sub from parameters if it exists
            string subParam = url.RequestContext.HttpContext.Request.QueryString["sub"]; // A subdomain specified as a query parameter takes precedence over the hostname.
            string fullHost = url.RequestContext.HttpContext.Request.Headers["Host"];

            // If the param is not being used, we will use the curSub
            if (string.IsNullOrEmpty(subParam))
            {
                // If we are in dev, we need to keep it in dev
                string firstSub = (curSub == "dev") ? "dev" : sub;
                rightUrl = url.Action(action, controller, Utility.Merge(new { sub = sub }, routeValues));

                domain = (string.IsNullOrEmpty(firstSub)) ? domain : firstSub + "." + domain;
            }
            else
            {
                //
                if (subParam != "dev")
                {
                    // replace the host and sub param in the context in order to generate the correct URLs
                    string newUrl = url.RequestContext.HttpContext.Request.Url.AbsoluteUri.SetUrlParameter("sub", sub);
                    url.RequestContext.HttpContext.RewritePath(url.RequestContext.HttpContext.Request.Path, url.RequestContext.HttpContext.Request.PathInfo, newUrl.GetUrlParameters());
                    // get the url for the new sub
                    rightUrl = url.Action(action, controller, routeValues);
                    var page = url.RequestContext.HttpContext.Handler as Page;
                    rightUrl = page.GetRouteUrl(new { sub = sub });
                    // Reset the url
                    string oldUrl = url.RequestContext.HttpContext.Request.Url.AbsoluteUri.SetUrlParameter("sub", subParam);
                    url.RequestContext.HttpContext.RewritePath(url.RequestContext.HttpContext.Request.Path, url.RequestContext.HttpContext.Request.PathInfo, newUrl.GetUrlParameters());
                }
                else // 'dev' is in the param, so we need to generate the action based on 
                {
                    rightUrl = url.Action(action, controller, routeValues);
                }

                // if using sub param, keep domain as is
                domain = host;
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
    }
}