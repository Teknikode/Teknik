using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Stats.Models;
using Teknik.Areas.Stats.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Piwik;
using Teknik.Utilities;

namespace Teknik.Areas.Stats.Controllers
{
    [TeknikAuthorize]
    public class StatsController : DefaultController
    {
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "System Statistics - " + Config.Title;
            ViewBag.Description = "Current statistics for the services.";

            StatsViewModel model = new StatsViewModel();

            using (TeknikEntities db = new TeknikEntities())
            {
                // Load initial status info
                #region Statistics
                model.UploadCount = db.Uploads.Count();
                model.UploadSize = db.Uploads.Sum(u => u.ContentLength);
                
                model.PasteCount = db.Pastes.Count();
                
                model.UserCount = db.Users.Count();
                
                model.ShortenedUrlCount = db.ShortenedUrls.Count();
                
                model.VaultCount = db.Vaults.Count();
                #endregion

                // Get Transaction Inforomation
                #region Transactions
                DateTime curTime = DateTime.Now;

                var billSums = db.Transactions.OfType<Bill>().GroupBy(b => new { b.Currency, b.DateSent.Month, b.DateSent.Year }).Select(b => new { month = b.Key.Month, year = b.Key.Year, currency = b.Key.Currency, total = b.Sum(c => c.Amount) }).ToList();
                foreach (var sum in billSums)
                {
                    decimal exchangeRate = CurrencyHelper.GetExchangeRate(sum.currency);
                    decimal realValue = sum.total * exchangeRate;
                    model.Transactions.TotalBills += realValue;
                    model.Transactions.TotalNet += realValue;
                    if (curTime.Month == sum.month && curTime.Year == sum.year)
                    {
                        model.Transactions.CurrentMonthBills += Math.Abs(realValue);
                    }
                }

                var oneSums = db.Transactions.OfType<OneTime>().GroupBy(b => new { b.Currency, b.DateSent.Month, b.DateSent.Year }).Select(b => new { month = b.Key.Month, year = b.Key.Year, currency = b.Key.Currency, total = b.Sum(c => c.Amount) }).ToList();
                foreach (var sum in oneSums)
                {
                    decimal exchangeRate = CurrencyHelper.GetExchangeRate(sum.currency);
                    decimal realValue = sum.total * exchangeRate;
                    model.Transactions.TotalOneTimes += realValue;
                    model.Transactions.TotalNet += realValue;
                    if (curTime.Month == sum.month && curTime.Year == sum.year)
                    {
                        model.Transactions.CurrentMonthBills += Math.Abs(realValue);
                    }
                }

                var donationSums = db.Transactions.OfType<Donation>().GroupBy(b => new { b.Currency, b.DateSent.Month, b.DateSent.Year }).Select(b => new { month = b.Key.Month, year = b.Key.Year, currency = b.Key.Currency, total = b.Sum(c => c.Amount) }).ToList();
                foreach (var sum in donationSums)
                {
                    decimal exchangeRate = CurrencyHelper.GetExchangeRate(sum.currency);
                    decimal realValue = sum.total * exchangeRate;
                    model.Transactions.TotalDonations += realValue;
                    model.Transactions.TotalNet += realValue;
                    if (curTime.Month == sum.month && curTime.Year == sum.year)
                    {
                        model.Transactions.CurrentMonthIncome += Math.Abs(realValue);
                    }
                }

                List<Bill> bills = db.Transactions.OfType<Bill>().OrderByDescending(b => b.DateSent).ToList();
                if (bills != null)
                {
                    foreach (Bill bill in bills)
                    {
                        BillViewModel billModel = new BillViewModel();
                        billModel.Amount = bill.Amount;
                        billModel.Currency = bill.Currency;
                        billModel.Reason = bill.Reason;
                        billModel.DateSent = bill.DateSent;
                        billModel.Recipient = bill.Recipient;
                        model.Transactions.Bills.Add(billModel);
                    }
                }

                List<OneTime> oneTimes = db.Transactions.OfType<OneTime>().OrderByDescending(b => b.DateSent).ToList();
                if (oneTimes != null)
                {
                    foreach (OneTime oneTime in oneTimes)
                    {
                        OneTimeViewModel oneTimeModel = new OneTimeViewModel();
                        oneTimeModel.Amount = oneTime.Amount;
                        oneTimeModel.Currency = oneTime.Currency;
                        oneTimeModel.Reason = oneTime.Reason;
                        oneTimeModel.DateSent = oneTime.DateSent;
                        oneTimeModel.Recipient = oneTime.Recipient;
                        model.Transactions.OneTimes.Add(oneTimeModel);
                    }
                }

                List<Donation> donations = db.Transactions.OfType<Donation>().OrderByDescending(b => b.DateSent).ToList();
                if (donations != null)
                {
                    foreach (Donation donation in donations)
                    {
                        DonationViewModel donationModel = new DonationViewModel();
                        donationModel.Amount = donation.Amount;
                        donationModel.Currency = donation.Currency;
                        donationModel.Reason = donation.Reason;
                        donationModel.DateSent = donation.DateSent;
                        donationModel.Sender = donation.Sender;
                        model.Transactions.Donations.Add(donationModel);
                    }
                }
                #endregion

                // Takedown information
                #region Takedowns
                List<Takedown> takedowns = db.Takedowns.OrderByDescending(b => b.DateRequested).ToList();
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
            }
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
