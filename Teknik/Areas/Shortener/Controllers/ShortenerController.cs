using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Shortener.ViewModels;
using Teknik.Controllers;
using Teknik.Models;

namespace Teknik.Areas.Shortener.Controllers
{
    public class ShortenerController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Url Shortener - " + Config.Title;
            ShortenViewModel model = new ShortenViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RedirectToUrl(string url)
        {
            ShortenedUrl shortUrl = db.ShortenedUrls.Where(s => s.ShortUrl == url).FirstOrDefault();
            if (shortUrl != null)
            {
                shortUrl.Views += 1;
                db.Entry(shortUrl).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToUrl(shortUrl.OriginalUrl);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ShortenUrl(string url)
        {
            if (url.IsValidUrl())
            {
                ShortenedUrl newUrl = Shortener.ShortenUrl(url, Config.ShortenerConfig.UrlLength);

                if (User.Identity.IsAuthenticated)
                {
                    Profile.Models.User foundUser = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                    if (foundUser != null)
                    {
                        newUrl.UserId = foundUser.UserId;
                    }
                }

                db.ShortenedUrls.Add(newUrl);
                db.SaveChanges();

                return Json(new { result = new { shortUrl = string.Format("http://{0}/{1}", Config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl), originalUrl = url } });
            }
            else
            {
                return Json(new { error = "Must be a valid HTTP or HTTPS Url" });
            }
        }
    }
}