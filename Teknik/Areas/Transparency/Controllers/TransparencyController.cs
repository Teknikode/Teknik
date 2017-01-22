using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Transparency.Models;
using Teknik.Areas.Transparency.ViewModels;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Models;

namespace Teknik.Areas.Transparency.Controllers
{
    [TeknikAuthorize]
    public class TransparencyController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Transparency - " + Config.Title;
            ViewBag.Description = "Teknik transparency and statistics.";
            TransparencyViewModel model = new TransparencyViewModel();

            if (Config.TransparencyConfig.Enabled)
            {
                Upload.Models.Upload upload = db.Uploads.OrderByDescending(u => u.UploadId).FirstOrDefault();
                model.UploadCount = (upload != null) ? upload.UploadId : 0;
                model.UploadSize = (upload != null) ? db.Uploads.Sum(u => (long)u.ContentLength) : 0;

                Paste.Models.Paste paste = db.Pastes.OrderByDescending(p => p.PasteId).FirstOrDefault();
                model.PasteCount = (paste != null) ? paste.PasteId : 0;

                Users.Models.User user = db.Users.OrderByDescending(u => u.UserId).FirstOrDefault();
                model.UserCount = (user != null) ? user.UserId : 0;

                Shortener.Models.ShortenedUrl url = db.ShortenedUrls.OrderByDescending(s => s.ShortenedUrlId).FirstOrDefault();
                model.ShortenedUrlCount = (url != null) ? url.ShortenedUrlId : 0;

                model.TotalNet = new Dictionary<string, double>();

                var billSums = db.Transactions.OfType<Bill>().GroupBy(b => b.Currency).Select(b => new { currency = b.Key, total = b.Sum(c => c.Amount) }).ToList();
                model.TotalBills = new Dictionary<string, double>();
                foreach (var sum in billSums)
                {
                    model.TotalBills.Add(sum.currency, sum.total);
                    if (model.TotalNet.ContainsKey(sum.currency))
                    {
                        model.TotalNet[sum.currency] += sum.total;
                    }
                    else
                    {
                        model.TotalNet.Add(sum.currency, sum.total);
                    }
                }

                var oneSums = db.Transactions.OfType<OneTime>().GroupBy(b => b.Currency).Select(b => new { currency = b.Key, total = b.Sum(c => c.Amount) }).ToList();
                model.TotalOneTimes = new Dictionary<string, double>();
                foreach (var sum in oneSums)
                {
                    model.TotalOneTimes.Add(sum.currency, sum.total);
                    if (model.TotalNet.ContainsKey(sum.currency))
                    {
                        model.TotalNet[sum.currency] += sum.total;
                    }
                    else
                    {
                        model.TotalNet.Add(sum.currency, sum.total);
                    }
                }

                var donationSums = db.Transactions.OfType<Donation>().GroupBy(b => b.Currency).Select(b => new { currency = b.Key, total = b.Sum(c => c.Amount) }).ToList();
                model.TotalDonations = new Dictionary<string, double>();
                foreach (var sum in donationSums)
                {
                    model.TotalDonations.Add(sum.currency, sum.total);
                    if (model.TotalNet.ContainsKey(sum.currency))
                    {
                        model.TotalNet[sum.currency] += sum.total;
                    }
                    else
                    {
                        model.TotalNet.Add(sum.currency, sum.total);
                    }
                }

                List<Bill> bills = db.Transactions.OfType<Bill>().OrderByDescending(b => b.DateSent).ToList();
                model.Bills = (bills != null) ? bills : new List<Bill>();

                List<OneTime> oneTimes = db.Transactions.OfType<OneTime>().OrderByDescending(b => b.DateSent).ToList();
                model.OneTimes = (oneTimes != null) ? oneTimes : new List<OneTime>();

                List<Donation> donations = db.Transactions.OfType<Donation>().OrderByDescending(b => b.DateSent).ToList();
                model.Donations = (donations != null) ? donations : new List<Donation>();


                List<Takedown> takedowns = db.Takedowns.OrderByDescending(b => b.DateRequested).ToList();
                model.Takedowns = (takedowns != null) ? takedowns : new List<Takedown>();

                // Grab canary file
                if (System.IO.File.Exists(Config.TransparencyConfig.CanaryPath))
                {
                    model.Canary = System.IO.File.ReadAllText(Config.TransparencyConfig.CanaryPath);
                }
                else
                {
                    model.Canary = string.Empty;
                }
            }
            return View(model);
        }
    }
}