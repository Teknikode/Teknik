using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Status.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Piwik;
using Teknik.Utilities;

namespace Teknik.Areas.Status.Controllers
{
    [TeknikAuthorize]
    public class StatusController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Status Information - " + Config.Title;
            ViewBag.Description = "Current status information for the server and resources.";

            StatusViewModel model = new StatusViewModel();

            // Load initial status info
            Upload.Models.Upload upload = db.Uploads.OrderByDescending(u => u.UploadId).FirstOrDefault();
            model.UploadCount = (upload != null) ? upload.UploadId : 0;
            model.UploadSize = (upload != null) ? db.Uploads.Sum(u => (long)u.ContentLength) : 0;

            Paste.Models.Paste paste = db.Pastes.OrderByDescending(p => p.PasteId).FirstOrDefault();
            model.PasteCount = (paste != null) ? paste.PasteId : 0;

            Users.Models.User user = db.Users.OrderByDescending(u => u.UserId).FirstOrDefault();
            model.UserCount = (user != null) ? user.UserId : 0;

            Shortener.Models.ShortenedUrl url = db.ShortenedUrls.OrderByDescending(s => s.ShortenedUrlId).FirstOrDefault();
            model.ShortenedUrlCount = (url != null) ? url.ShortenedUrlId : 0;

            Vault.Models.Vault vault = db.Vaults.OrderByDescending(v => v.VaultId).FirstOrDefault();
            model.VaultCount = (url != null) ? vault.VaultId : 0;

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetVisitorData()
        {
            // Get the data from the Piwik 
            if (!string.IsNullOrEmpty(Config.PiwikConfig.API))
            {
                List<VisitorData> dataList = Reporting.GetVisitSummaryByDays(Config, 31);

                List<object> uniqueData = new List<object>();
                List<object> totalData = new List<object>();

                foreach (VisitorData data in dataList.OrderBy(d => d.Date))
                {
                    object uniqueDay = new { x = Convert.ToInt64((data.Date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds), y = data.UniqueVisitors };
                    uniqueData.Add(uniqueDay);
                    object totalDay = new { x = Convert.ToInt64((data.Date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds), y = data.Visits };
                    totalData.Add(totalDay);
                }

                return Json(new { result = new { uniqueVisitors = uniqueData.ToArray(), totalVisitors = totalData.ToArray() } }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { error = new { message = "Piwik not configured" } }, JsonRequestBehavior.AllowGet);
        }
    }
}