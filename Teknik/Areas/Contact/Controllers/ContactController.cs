using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Controllers;

namespace Teknik.Areas.Contact.Controllers
{
    public class ContactController : DefaultController
    {
        // GET: Contact/Contact
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = Config.Title + " - Contact";
            ViewBag.Message = "Contact Teknik Support";

            return View();
        }
    }
}