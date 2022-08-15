using System;
using System.Net.Mail;
using Teknik.Controllers;
using Teknik.Areas.Contact.ViewModels;
using System.Text;
using Teknik.Attributes;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Teknik.Logging;
using Teknik.Areas.Users.Utility;
using Teknik.MailService;

namespace Teknik.Areas.Contact.Controllers
{
    [Authorize]
    [Area("Contact")]
    public class ContactController : DefaultController
    {
        public ContactController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index([FromServices] IMailService mailService)
        {
            ViewBag.Title = "Contact Us";
            ViewBag.Description = "Contact Teknik Support";
            var model = new ContactViewModel();

            if (User.Identity.IsAuthenticated)
            {
                model.Name = User.Identity.Name;

                var email = UserHelper.GetUserEmailAddress(_config, User.Identity.Name);
                if (UserHelper.UserEmailEnabled(mailService, _config, email))
                {
                    model.Email = email;
                }
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Submit(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_config.ContactConfig.Enabled)
                {
                    try
                    {
                        // Insert the message into the DB
                        Models.Contact newContact = new Models.Contact();
                        newContact.Name = model.Name;
                        newContact.Email = model.Email;
                        newContact.Subject = model.Subject;
                        newContact.Message = model.Message;
                        newContact.DateAdded = DateTime.Now;
                        _dbContext.Contact.Add(newContact);
                        _dbContext.SaveChanges();

                        // Let's also email the message to support
                        SmtpClient client = new SmtpClient();
                        client.Host = _config.ContactConfig.EmailAccount.Host;
                        client.Port = _config.ContactConfig.EmailAccount.Port;
                        client.EnableSsl = _config.ContactConfig.EmailAccount.SSL;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.Credentials = new System.Net.NetworkCredential(_config.ContactConfig.EmailAccount.Username, _config.ContactConfig.EmailAccount.Password);
                        client.Timeout = 5000;

                        MailMessage mail = new MailMessage(new MailAddress(model.Email, model.Name), new MailAddress(_config.SupportEmail, "Teknik Support"));
                        mail.Sender = new MailAddress(_config.ContactConfig.EmailAccount.EmailAddress);
                        mail.Subject = model.Subject;
                        mail.Body = model.Message;
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
