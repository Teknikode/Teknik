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
using Teknik.Filters;
using Teknik.Attributes;

namespace Teknik.Areas.Contact.Controllers
{
    [TeknikAuthorize]
    public class ContactController : DefaultController
    {
        // GET: Contact/Contact
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Contact - " + Config.Title;
            ViewBag.Description = "Contact Teknik Support";

            return View(new ContactViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Submit(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Config.ContactConfig.Enabled)
                {
                    try
                    {
                        using (TeknikEntities db = new TeknikEntities())
                        {
                            // Insert the message into the DB
                            Models.Contact newContact = db.Contact.Create();
                            newContact.Name = model.Name;
                            newContact.Email = model.Email;
                            newContact.Subject = model.Subject;
                            newContact.Message = model.Message;
                            newContact.DateAdded = DateTime.Now;
                            db.Contact.Add(newContact);
                            db.SaveChanges();
                        }

                        // Let's also email the message to support
                        SmtpClient client = new SmtpClient();
                        client.Host = Config.ContactConfig.EmailAccount.Host;
                        client.Port = Config.ContactConfig.EmailAccount.Port;
                        client.EnableSsl = Config.ContactConfig.EmailAccount.SSL;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = true;
                        client.Credentials = new System.Net.NetworkCredential(Config.ContactConfig.EmailAccount.Username, Config.ContactConfig.EmailAccount.Password);
                        client.Timeout = 5000;

                        MailMessage mail = new MailMessage(Config.ContactConfig.EmailAccount.EmailAddress, Config.SupportEmail);
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
                        return Json(new { error = "Error submitting message. Exception: " + ex.Message });
                    }

                    return Json(new { result = "true" });
                }
                return Json(new { error = "Contact Form is disabled" });
            }
            else
            {
                return View(model);
            }
        }
    }
}