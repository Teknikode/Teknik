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

namespace Teknik.Filters
{
    public class TrackDownload : ActionFilterAttribute
    {
        private readonly Config _config;

        public TrackDownload(Config config)
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
                string userAgent = request.Headers["User-Agent"].ToString();

                string clientIp = request.ClientIPFromRequest(true);

                string urlReferrer = request.Headers["Referer"].ToString();

                string url = UriHelper.GetEncodedUrl(request);

                // Fire and forget.  Don't need to wait for it.
                Tracking.Tracking.TrackDownload(filterContext.HttpContext, _config, userAgent, clientIp, url, urlReferrer);
            }
        }
    }
}
