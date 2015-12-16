using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using Teknik.Controllers;
using Teknik.Areas.Contact.ViewModels;
using Teknik.Areas.Contact.Models;
using Teknik.Models;
using System.Text;

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
                    try
                    {
                        // Let's also email the message to support
                        SmtpClient client = new SmtpClient();
                        client.Host = Config.SMTPConfig.Host;
                        client.Port = Config.SMTPConfig.Port;
                        client.EnableSsl = Config.SMTPConfig.SSL;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = true;
                        client.Credentials = new System.Net.NetworkCredential(Config.SMTPConfig.Username, Config.SMTPConfig.Password);
                        client.Timeout = 5000;

                        MailMessage mail = new MailMessage(Config.SupportEmail, Config.SupportEmail);
                        mail.Subject = string.Format("Support Message from: {0} <{1}>", model.Name, model.Email);
                        mail.Body = string.Format(@"
New Support Message from: {0} <{1}>
                                                
---------------------------------
Subject: {2}
Message: {3}", model.Name, model.Email, model.Subject, model.Message);
                        mail.BodyEncoding = UTF8Encoding.UTF8;
                        mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                        client.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { error = "Error submitting message. Exception: " +  ex.Message});
                    }

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