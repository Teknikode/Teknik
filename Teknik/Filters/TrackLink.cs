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
            Config config = Config.Load();

            if (config.PiwikConfig.Enabled)
            {
                HttpRequestBase request = filterContext.HttpContext.Request;

                string doNotTrack = request.Headers["DNT"];
                bool dnt = (string.IsNullOrEmpty(doNotTrack) || doNotTrack != "1");

                string userAgent = request.UserAgent;

                string ipAddress = request.ClientIPFromRequest(true);

                string urlReferrer = request.UrlReferrer?.ToString();

                string url = string.Empty;
                if (request.Url != null)
                    url = request.Url.ToString();

                // Fire and forget.  Don't need to wait for it.
                Task.Run(() => AsyncTrackLink(dnt, config.PiwikConfig.SiteId, config.PiwikConfig.Url, userAgent, ipAddress, config.PiwikConfig.TokenAuth, url, urlReferrer));
            }

            base.OnActionExecuted(filterContext);
        }

        private void AsyncTrackLink(bool dnt, int siteId, string siteUrl, string userAgent, string clientIp, string token, string url, string urlReferrer)
        {
            Tracking.TrackLink(dnt, siteId, siteUrl, userAgent, clientIp, token, url, urlReferrer);
        }
    }
}
