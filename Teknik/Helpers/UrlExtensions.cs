using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

            string firstSub = string.Empty;
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
            
            // If the param is not being used, we will use the curSub
            if (string.IsNullOrEmpty(subParam))
            {
                // If we are in dev, we need to keep it in dev
                firstSub = (curSub == "dev") ? "dev" : sub;
                rightUrl = url.Action(action, controller, routeValues);
            }
            else
            {
                // sub within param will always be on the dev subdomain
                firstSub = (string.IsNullOrEmpty(curSub)) ? string.Empty : "dev";
                if (subParam != "dev")
                {
                    rightUrl = url.Action(action, controller, Utility.Merge(new { sub = sub }, routeValues));
                }
                else
                {
                    rightUrl = url.Action(action, controller, routeValues);
                }
            }

            domain = (string.IsNullOrEmpty(firstSub)) ? domain : firstSub + "." + domain;

            string absoluteAction = string.Format("{0}://{1}{2}", url.RequestContext.HttpContext.Request.Url.Scheme, domain, rightUrl);
  
            return absoluteAction;
        }
    }
}