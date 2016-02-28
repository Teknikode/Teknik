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
            string title = string.Empty;
            Page page = filterContext.HttpContext.Handler as Page;

            if (page != null)
            {
                title = page.Title;
            }
            Config config = Config.Load();
            Tracking.TrackPageView(filterContext.HttpContext.Request, config, title);

            base.OnActionExecuted(filterContext);
        }

    }
}
