using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Upload;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Attributes;

namespace Teknik.Areas.API.Controllers
{
    [TeknikAuthorize]
    public class APIController : DefaultController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.API"));
        }
    }
}