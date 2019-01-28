using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Tracking;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Teknik.Filters
{
    public class TrackPageView : ActionFilterAttribute
    {
        private readonly Config _config;

        public TrackPageView(Config config)
        {
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            HttpRequest request = filterContext.HttpContext.Request;

            string doNotTrack = request.Headers["DNT"];
            if (string.IsNullOrEmpty(doNotTrack) || doNotTrack != "1")
            {
                string title = (filterContext.Controller as Controller)?.ViewBag?.Title;

                string sub = filterContext.RouteData.Values["sub"].ToString();
                if (string.IsNullOrEmpty(sub))
                {
                    sub = request.Host.ToUriComponent().GetSubdomain();
                }

                string clientIp = request.ClientIPFromRequest(true);

                string url = UriHelper.GetEncodedUrl(request);

                string urlReferrer = request.Headers["Referer"].ToString();

                string userAgent = request.Headers["User-Agent"].ToString();

                int pixelWidth = 0;
                int pixelHeight = 0;

                bool hasCookies = false;

                string acceptLang = request.Headers["Accept-Language"];

                bool hasJava = false;

                // Fire and forget.  Don't need to wait for it.
                Tracking.Tracking.TrackPageView(filterContext.HttpContext, _config, title, sub, clientIp, url, urlReferrer, userAgent, pixelWidth, pixelHeight, hasCookies, acceptLang, hasJava);
            }
        }
    }
}
