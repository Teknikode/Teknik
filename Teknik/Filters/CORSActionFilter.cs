using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teknik.Filters
{
    public class CORSActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Method == "OPTIONS")
            {
                // do nothing let IIS deal with reply!
                filterContext.Result = new EmptyResult();
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }
    }
}