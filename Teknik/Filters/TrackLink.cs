using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.Configuration;
using Teknik.Piwik;
using Teknik.Utilities;

namespace Teknik.Filters
{
    public class TrackLink : ActionFilterAttribute
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
                Tracking.TrackLink(userAgent, clientIp, url, urlReferrer);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
