using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Teknik.Areas.Stats.Models;
using Teknik.Areas.Stats.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Tracking;

namespace Teknik.Areas.Stats.Controllers
{
    [Authorize]
    [Area("Stats")]
    public class StatsController : DefaultController
    {
        public StatsController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index()
        {
            ViewBag.Title = "System Statistics";
            ViewBag.Description = "Current statistics for the services.";

            StatsViewModel model = new StatsViewModel();

            // Load initial status info
            #region Statistics
            model.UploadCount = _dbContext.Uploads.Count();
            model.UploadSize = _dbContext.Uploads.Sum(u => u.ContentLength);

            model.PasteCount = _dbContext.Pastes.Count();

            model.UserCount = _dbContext.Users.Count();

            model.ShortenedUrlCount = _dbContext.ShortenedUrls.Count();

            model.VaultCount = _dbContext.Vaults.Count();
            #endregion

            // Takedown information
            #region Takedowns
            List<Takedown> takedowns = _dbContext.Takedowns.OrderByDescending(b => b.DateRequested).ToList();
            if (takedowns != null)
            {
                foreach (Takedown takedown in takedowns)
                {
                    TakedownViewModel takedownModel = new TakedownViewModel();
                    takedownModel.Requester = takedown.Requester;
                    takedownModel.RequesterContact = takedown.RequesterContact;
                    takedownModel.Reason = takedown.Reason;
                    takedownModel.ActionTaken = takedown.ActionTaken;
                    takedownModel.DateRequested = takedown.DateRequested;
                    takedownModel.DateActionTaken = takedown.DateActionTaken;

                    model.Takedowns.Add(takedownModel);
                }
            }
            #endregion
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetVisitorData()
        {
            // Get the data from the Piwik 
            if (!string.IsNullOrEmpty(_config.PiwikConfig.API))
            {
                List<VisitorData> dataList = Reporting.GetVisitSummaryByDays(_config, 31);

                List<object> uniqueData = new List<object>();
                List<object> totalData = new List<object>();

                foreach (VisitorData data in dataList.OrderBy(d => d.Date))
                {
                    object uniqueDay = new { x = Convert.ToInt64((data.Date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds), y = data.UniqueVisitors };
                    uniqueData.Add(uniqueDay);
                    object totalDay = new { x = Convert.ToInt64((data.Date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds), y = data.Visits };
                    totalData.Add(totalDay);
                }

                return Json(new { result = new { uniqueVisitors = uniqueData.ToArray(), totalVisitors = totalData.ToArray() } });
            }
            return Json(new { error = new { message = "Tracking not configured" } });
        }
    }
}
