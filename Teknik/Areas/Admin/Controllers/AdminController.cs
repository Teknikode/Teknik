using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Admin.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.Controllers
{
    [TeknikAuthorize(Roles = "Admin")]
    public class AdminController : DefaultController
    {
        public ActionResult Dashboard()
        {
            DashboardViewModel model = new DashboardViewModel();
            return View(model);
        }

        public ActionResult Search()
        {
            SearchViewModel model = new SearchViewModel();
            return View(model);
        }
    }
}