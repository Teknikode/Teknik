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
using Teknik.Utilities;
using Teknik.Logging;
using Teknik.Attributes;

namespace Teknik.Areas.Error.Controllers
{
    [TeknikAuthorize]
    public class ErrorController : DefaultController
    {
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

            LogError(LogLevel.Error, "General Exception", exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Exception.cshtml", model);
        }
        
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

            LogError(LogLevel.Error, "General HTTP Exception", exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Description = exception.Message;
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/General.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Http401(Exception exception)
        {
            ViewBag.Title = "401 - " + Config.Title;
            ViewBag.Description = "Unauthorized";

            if (Response != null)
            {
                Response.StatusCode = 401;
                Response.TrySkipIisCustomErrors = true;
            }

            LogError(LogLevel.Error, "Unauthorized", exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http401.cshtml", model);
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

            LogError(LogLevel.Error, "Access Denied", exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http403.cshtml", model);
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

            LogError(LogLevel.Warning, "Page Not Found", exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http404.cshtml", model);
        }
        
        [AllowAnonymous]
        public ActionResult Http500(Exception exception)
        {
            Session["Exception"] = exception;

            ViewBag.Title = "500 - " + Config.Title;
            ViewBag.Description = "Something Borked";

            if (Response != null)
            {
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
            }
            
            LogError(LogLevel.Error, "Server Error", exception);

            ErrorViewModel model = new ErrorViewModel();
            model.Exception = exception;

            return View("~/Areas/Error/Views/Error/Http500.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitErrorReport(SubmitReportViewModel model)
        {
            try
            {
                string exceptionMsg = model.Exception;

                // Try to grab the actual exception that occured
                object exceptionObj = Session["Exception"];
                if (exceptionObj != null)
                {
                    Exception ex = (Exception) exceptionObj;
                    exceptionMsg = string.Format(@"
Exception: {0}

Source: {1}

Stack Trace:

{2}
", ex.GetFullMessage(true), ex.Source, ex.StackTrace);
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

                MailMessage mail = new MailMessage(new MailAddress(Config.NoReplyEmail, Config.NoReplyEmail), new MailAddress(Config.SupportEmail, "Teknik Support"));
                mail.Sender = new MailAddress(Config.ContactConfig.EmailAccount.EmailAddress);
                mail.Subject = "[Exception] Application Exception Occured";
                mail.Body = @"
An exception has occured at: " + model.CurrentUrl + @"

----------------------------------------
User Message:

" + model.Message + @"

----------------------------------------
" + exceptionMsg;
                mail.BodyEncoding = UTF8Encoding.UTF8;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                client.Send(mail);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Error submitting report. Exception: " + ex.Message });
            }

            return Json(new { result = "true" });
        }

        private void LogError(LogLevel level, string message, Exception exception)
        {
            if (Request != null)
            {
                if (Request.Url != null)
                {
                    message += " | Url: " + Request.Url.AbsoluteUri;
                }

                if (Request.UrlReferrer != null)
                {
                    message += " | Referred Url: " + Request.Url.AbsoluteUri;
                }
                
                message += " | Method: " + Request.HttpMethod;

                message += " | User Agent: " + Request.UserAgent;
            }

            Logger.WriteEntry(level, message, exception);
        }
    }
}
