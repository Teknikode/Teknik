using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Error.ViewModels;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Helpers;

namespace Teknik.Areas.Error.Controllers
{
    public class ErrorController : DefaultController
    {
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Exception(Exception exception)
        {
            ViewBag.Title = "Exception - " + Config.Title;
            ViewBag.Description = "Just a boring 'ol exception. Nothing to see here, move along.";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }

            if (exception != null)
            {
                SendErrorEmail(exception);
            }

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("/Areas/Error/Views/Error/Exception.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult General(Exception exception)
        {
            ViewBag.Title = "Http Exception - " + Config.Title;
            ViewBag.Description = "There has been a Http exception.  Run!";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }

            if (exception != null)
            {
                SendErrorEmail(exception);
            }

            ErrorViewModel model = new ErrorViewModel();
            model.Description = exception.Message;
            model.Exception = exception;

            return View("/Areas/Error/Views/Error/General.cshtml", model);
        }
        
        [AllowAnonymous]
        public ActionResult Http403(Exception exception)
        {
            ViewBag.Title = "403 - " + Config.Title;
            ViewBag.Description = "Access Denied";

            if (Response != null)
            {
                Response.StatusCode = 403;
                Response.TrySkipIisCustomErrors = true;
            }

            if (exception != null)
            {
                SendErrorEmail(exception);
            }

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("/Areas/Error/Views/Error/Http403.cshtml", model);
        }
        
        [AllowAnonymous]
        public ActionResult Http404(Exception exception)
        {
            ViewBag.Title = "404 - " + Config.Title;
            ViewBag.Description = "Uh Oh, can't find it!";

            if (Response != null)
            {
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;
            }

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("/Areas/Error/Views/Error/Http404.cshtml", model);
        }

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Http500(Exception exception)
        {
            ViewBag.Title = "500 - " + Config.Title;
            ViewBag.Description = "Something Borked";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }

            if (exception != null)
            {
                SendErrorEmail(exception);
            }

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("/Areas/Error/Views/Error/Http500.cshtml", model);
        }

        private void SendErrorEmail(Exception ex)
        {
            try
            {
                // Let's also email the message to support
                SmtpClient client = new SmtpClient();
                client.Host = Config.ContactConfig.Host;
                client.Port = Config.ContactConfig.Port;
                client.EnableSsl = Config.ContactConfig.SSL;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = true;
                client.Credentials = new System.Net.NetworkCredential(Config.ContactConfig.Username, Config.ContactConfig.Password);
                client.Timeout = 5000;

                MailMessage mail = new MailMessage(Config.SupportEmail, Config.SupportEmail);
                mail.Subject = string.Format("Exception Occured on: {0}", Request.Url.AbsoluteUri);
                mail.Body = string.Format(@"
Message: {0}

Source: {1}

Stack Trace: {2}", ex.GetFullMessage(true), ex.Source, ex.StackTrace);
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                client.Send(mail);
            }
            catch (Exception) { /* don't handle something in the handler */ }
        }
    }
}