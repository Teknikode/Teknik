using System.Web;
using System.Web.Mvc;
using Teknik.Attributes;
using Teknik.Filters;

namespace Teknik
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            //filters.Add(new TeknikAuthorizeAttribute());
            filters.Add(new RequireHttpsAttribute());
        }
    }
}
