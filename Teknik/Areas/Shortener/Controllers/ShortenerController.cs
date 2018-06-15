using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Shortener.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.Areas.Shortener.Controllers
{
    [TeknikAuthorize]
    [Area("Shortener")]
    public class ShortenerController : DefaultController
    {
        public ShortenerController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [TrackPageView]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Url Shortener - " + _config.Title;
            ShortenViewModel model = new ShortenViewModel();
            return View(model);
        }

        [TrackLink]
        [AllowAnonymous]
        public IActionResult RedirectToUrl(string url)
        {
            ShortenedUrl shortUrl = _dbContext.ShortenedUrls.Where(s => s.ShortUrl == url).FirstOrDefault();
            if (shortUrl != null)
            {
                shortUrl.Views += 1;
                _dbContext.Entry(shortUrl).State = EntityState.Modified;
                _dbContext.SaveChanges();
                return Redirect(shortUrl.OriginalUrl);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ShortenUrl(string url)
        {
            if (url.IsValidUrl())
            {
                ShortenedUrl newUrl = ShortenerHelper.ShortenUrl(_dbContext, url, _config.ShortenerConfig.UrlLength);

                if (User.Identity.IsAuthenticated)
                {
                    Users.Models.User foundUser = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (foundUser != null)
                    {
                        newUrl.UserId = foundUser.UserId;
                    }
                }

                _dbContext.ShortenedUrls.Add(newUrl);
                _dbContext.SaveChanges();

                string shortUrl = string.Format("{0}://{1}/{2}", Request.Scheme, _config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl);
                if (_config.DevEnvironment)
                {
                    shortUrl = Url.SubRouteUrl("shortened", "Shortener.View", new { url = newUrl.ShortUrl });
                }

                return Json(new { result = new { shortUrl = shortUrl, originalUrl = url } });
            }
            return Json(new { error = "Must be a valid Url" });
        }

        [AllowAnonymous]
        public IActionResult Verify()
        {
            ViewBag.Title = "Url Shortener Verification - " + _config.Title;
            return View();
        }
    }
}
