using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Shortener.Models;
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
            return View();
        }

        [AllowAnonymous]
        public ActionResult RedirectToUrl(string url)
        {
            ShortenedUrl shortUrl = db.ShortenedUrls.Where(s => s.ShortUrl == url).FirstOrDefault();
            if (shortUrl != null)
            {
                return RedirectToUrl(shortUrl.OriginalUrl);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ShortenUrl(string url)
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

            return Json(new { result = new { shortUrl = newUrl.ShortUrl, originalUrl = url } });
        }
    }
}