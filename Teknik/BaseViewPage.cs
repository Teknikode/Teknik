using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Security;

namespace Teknik
{
    public abstract class BaseViewPage : WebViewPage
    {
        public virtual new TeknikPrincipal User
        {
            get { return base.User as TeknikPrincipal; }
        }
    }

    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new TeknikPrincipal User
        {
            get { return base.User as TeknikPrincipal; }
        }
    }
}