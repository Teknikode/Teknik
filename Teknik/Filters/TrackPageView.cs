using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Piwik;

namespace Teknik.Filters
{
    public class TrackPageView : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;

            string doNotTrack = request.Headers["DNT"];
            if (string.IsNullOrEmpty(doNotTrack) || doNotTrack != "1")
            {
                string title = filterContext.Controller?.ViewBag?.Title;

                string sub = request.RequestContext.RouteData.Values["sub"].ToString();
                if (string.IsNullOrEmpty(sub))
                {
                    sub = request.Url?.AbsoluteUri.GetSubdomain();
                }

                string clientIp = request.ClientIPFromRequest(true);

                string url = string.Empty;
                if (request.Url != null)
                    url = request.Url.ToString();

                string urlReferrer = request.UrlReferrer?.ToString();

                string userAgent = request.UserAgent;

                int pixelWidth = request.Browser.ScreenPixelsWidth;
                int pixelHeight = request.Browser.ScreenPixelsHeight;

                bool hasCookies = request.Browser.Cookies;

                string acceptLang = request.Headers["Accept-Language"];

                bool hasJava = request.Browser.JavaApplets;

                // Fire and forget.  Don't need to wait for it.
                Tracking.TrackPageView(title, sub, clientIp, url, urlReferrer, userAgent, pixelWidth, pixelHeight, hasCookies, acceptLang, hasJava);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
