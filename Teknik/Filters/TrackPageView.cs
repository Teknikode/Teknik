using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Piwik;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Teknik.Filters
{
    public class TrackPageView : ActionFilterAttribute
    {
        public TrackPageView()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //HttpRequest request = filterContext.HttpContext.Request;

            //string doNotTrack = request.Headers["DNT"];
            //if (string.IsNullOrEmpty(doNotTrack) || doNotTrack != "1")
            //{
            //    string title = filterContext.Controller?.ViewBag?.Title;

            //    string sub = filterContext.RouteData.Values["sub"].ToString();
            //    if (string.IsNullOrEmpty(sub))
            //    {
            //        sub = request.Host.ToUriComponent().GetSubdomain();
            //    }

            //    string clientIp = request.ClientIPFromRequest(true);

            //    string url = UriHelper.GetEncodedUrl(request);

            //    string urlReferrer = request.Headers["Referer"].ToString();

            //    string userAgent = request.Headers["User-Agent"].ToString();

            //    int pixelWidth = request.Browser.ScreenPixelsWidth;
            //    int pixelHeight = request.Browser.ScreenPixelsHeight;

            //    bool hasCookies = request.Browser.Cookies;

            //    string acceptLang = request.Headers["Accept-Language"];

            //    bool hasJava = request.Browser.JavaApplets;

            //    // Fire and forget.  Don't need to wait for it.
            //    Tracking.TrackPageView(_config, title, sub, clientIp, url, urlReferrer, userAgent, pixelWidth, pixelHeight, hasCookies, acceptLang, hasJava);
            //}
        }
    }
}
