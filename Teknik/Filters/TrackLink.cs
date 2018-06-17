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

namespace Teknik.Filters
{
    public class TrackLink : ActionFilterAttribute
    {
        public TrackLink()
        {
            //_config = config;
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
            //    string userAgent = request.Headers["User-Agent"].ToString();

            //    string clientIp = request.ClientIPFromRequest(true);

            //    string urlReferrer = request.Headers["Referer"].ToString();

            //    string url = UriHelper.GetEncodedUrl(request);

            //    // Fire and forget.  Don't need to wait for it.
            //    Tracking.TrackLink(_config, userAgent, clientIp, url, urlReferrer);
            //}
        }
    }
}
