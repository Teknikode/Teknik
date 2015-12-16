using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Controllers;
using Teknik.Areas.Contact.ViewModels;
using Teknik.Areas.Contact.Models;
using Teknik.Models;

namespace Teknik.Areas.Contact.Controllers
{
    public class ContactController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Contact/Contact
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = Config.Title + " - Contact";
            ViewBag.Message = "Contact Teknik Support";

            return View(new ContactViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Submit(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Insert())
                {
                    return Json(new { result = "true" });
                }
                return Json(new { error = "Error submitting message." });
            }
            else
            {
                return View(model);
            }
        }
    }
}