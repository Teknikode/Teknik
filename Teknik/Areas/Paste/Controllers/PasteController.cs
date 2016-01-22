using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Controllers;

namespace Teknik.Areas.Paste.Controllers
{
    public class PasteController : DefaultController
    {
        // GET: Paste/Paste
        public ActionResult Index()
        {
            return View();
        }
    }
}