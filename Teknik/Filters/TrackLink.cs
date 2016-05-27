using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Teknik.Configuration;
using Teknik.Helpers;

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
            // Fire and forget.  Don't need to wait for it.
            Task.Run(() => AsyncTrackLink(request, request.Url.ToString()));

            base.OnActionExecuted(filterContext);
        }

        private void AsyncTrackLink(HttpRequestBase request, string url)
        {
            Config config = Config.Load();
            Tracking.TrackLink(request, config, url);
        }
    }
}
