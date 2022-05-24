using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Teknik.Configuration;
using Teknik.Tracking;
using Teknik.Utilities;

namespace Teknik.Attributes
{
    public class TrackLink : ActionFilterAttribute
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly Config _config;

        public TrackLink(IBackgroundTaskQueue queue, Config config)
        {
            _queue = queue;
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

                string url = request.GetEncodedUrl();

                string tokenAuth = _config.PiwikConfig.TokenAuth;
                int siteId = _config.PiwikConfig.SiteId;
                string apiUrl = _config.PiwikConfig.Url;

                // Fire and forget.  Don't need to wait for it.
                _queue.QueueBackgroundWorkItem(async token =>
                {
                    await Task.Run(() =>
                    {
                        Tracking.Tracking.TrackLink(tokenAuth, 
                                                    siteId,
                                                    apiUrl,
                                                    userAgent, 
                                                    clientIp, 
                                                    url, 
                                                    urlReferrer);
                    });
                });
            }
        }
    }
}
