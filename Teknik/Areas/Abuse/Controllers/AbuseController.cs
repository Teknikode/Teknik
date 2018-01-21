using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Abuse.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;

namespace Teknik.Areas.Abuse.Controllers
{
    [TeknikAuthorize]
    public class AbuseController : DefaultController
    {
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Abuse Reporting - " + Config.Title;
            ViewBag.Description = "Methods for reporting abuse reports to Teknik Services.";

            return View(new AbuseViewModel());
        }
    }
}
