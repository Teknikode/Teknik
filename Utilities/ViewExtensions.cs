namespace Teknik.Utilities
{
    public static class ViewExtensions
    {
        //public static string RenderToString(this PartialViewResult partialView)
        //{
        //    var httpContext = HttpContext.Current;

        //    if (httpContext == null)
        //    {
        //        throw new NotSupportedException("An HTTP context is required to render the partial view to a string");
        //    }

        //    var controllerName = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();

        //    var controller = (ControllerBase)ControllerBuilder.Current.GetControllerFactory().CreateController(httpContext.Request.RequestContext, controllerName);

        //    var controllerContext = new ControllerContext(httpContext.Request.RequestContext, controller);

        //    var view = ViewEngines.Engines.FindPartialView(controllerContext, partialView.ViewName).View;

        //    var sb = new StringBuilder();

        //    using (var sw = new StringWriter(sb))
        //    {
        //        using (var tw = new HtmlTextWriter(sw))
        //        {
        //            view.Render(new ViewContext(controllerContext, view, partialView.ViewData, partialView.TempData, tw), tw);
        //        }
        //    }

        //    return sb.ToString();
        //}
    }
}
