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
    public class TrackDownload : ActionFilterAttribute
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
                string userAgent = request.UserAgent;

                string clientIp = request.ClientIPFromRequest(true);

                string urlReferrer = request.UrlReferrer?.ToString();

                string url = string.Empty;
                if (request.Url != null)
                    url = request.Url.ToString();

                // Fire and forget.  Don't need to wait for it.
                Tracking.TrackDownload(userAgent, clientIp, url, urlReferrer);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
