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
        public ActionResult ShortenUrl(string url)
        {

            return Json(new { result = true });
        }
    }
}