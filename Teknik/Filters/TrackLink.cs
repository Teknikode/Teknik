using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Teknik.Configuration;
using Teknik.Piwik;

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

                // Fire and forget.  Don't need to wait for it.
                Task.Run(() => AsyncTrackLink(request, config, request.Url?.ToString()));
            }

            base.OnActionExecuted(filterContext);
        }

        private void AsyncTrackLink(HttpRequestBase request, Config config, string url)
        {
            Tracking.TrackLink(request, config, url);
        }
    }
}
