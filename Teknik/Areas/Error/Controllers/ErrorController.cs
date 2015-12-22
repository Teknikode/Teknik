using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Controllers;

namespace Teknik.Areas.Error.Controllers
{
    public class ErrorController : DefaultController
    {
        // GET: Error/Error
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Http404()
        {
            return View();
        }
    }
}