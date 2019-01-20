using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.API.Controllers;
using Teknik.Areas.API.V1.Models;
using Teknik.Areas.Shortener;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.Areas.API.V1.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "WriteOnlyAPI")]
    public class ShortenAPIv1Controller : APIv1Controller
    {
        public ShortenAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Shorten(ShortenAPIv1Model model)
        {
            try
            {
                if (model.url.IsValidUrl())
                {
                    ShortenedUrl newUrl = ShortenerHelper.ShortenUrl(_dbContext, model.url, _config.ShortenerConfig.UrlLength);

                    // Associate this with the user if they are logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        User foundUser = UserHelper.GetUser(_dbContext, User.Identity.Name);
                        if (foundUser != null)
                        {
                            newUrl.UserId = foundUser.UserId;
                        }
                    }

                    _dbContext.ShortenedUrls.Add(newUrl);
                    _dbContext.SaveChanges();

                    string shortUrl = string.Format("{0}://{1}/{2}", HttpContext.Request.Scheme, _config.ShortenerConfig.ShortenerHost, newUrl.ShortUrl);
                    if (_config.DevEnvironment)
                    {
                        shortUrl = Url.SubRouteUrl("shortened", "Shortener.View", new { url = newUrl.ShortUrl });
                    }

                    return Json(new
                    {
                        result = new
                        {
                            shortUrl = shortUrl,
                            originalUrl = model.url
                        }
                    });
                }
                return Json(new { error = new { message = "Must be a valid Url" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }
    }
}