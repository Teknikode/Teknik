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
    public class TrackingFilterAttribute : ActionFilterAttribute
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
                try
                {
                    string sub = filterContext.HttpContext.Request.RequestContext.RouteData.Values["sub"].ToString();
                    if (string.IsNullOrEmpty(sub))
                    {
                        sub = filterContext.HttpContext.Request.Url.AbsoluteUri.GetSubdomain();
                    }
                    string title = config.Title;
                    Page page = filterContext.HttpContext.Handler as Page;

                    if (page != null)
                    {
                        title = page.Title;
                    }
                    Tracking.TrackPageView(filterContext.HttpContext.Request, title, sub);
                }
                catch (Exception ex)
                {

                }
            }

            base.OnActionExecuted(filterContext);
        }

    }
}
