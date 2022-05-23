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

namespace Teknik.Attributes
{
    public class TrackDownloadAttribute : TypeFilterAttribute
    {
        public TrackDownloadAttribute() : base(typeof(TrackDownload))
        {
        }

        public class TrackDownload : ActionFilterAttribute
        {
            private readonly IBackgroundTaskQueue _queue;
            private readonly Config _config;

            public TrackDownload(IBackgroundTaskQueue queue, Config config)
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

                    // Fire and forget.  Don't need to wait for it.
                    _queue.QueueBackgroundWorkItem(async token =>
                    {
                        await Task.Run(() =>
                        {
                            Tracking.Tracking.TrackDownload(filterContext.HttpContext, _config, userAgent, clientIp, url, urlReferrer);
                        });
                    });
                }
            }
        }
    }
}
