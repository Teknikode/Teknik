using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.Privacy.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;

namespace Teknik.Areas.Privacy.Controllers
{
    [TeknikAuthorize]
    [Area("Privacy")]
    public class PrivacyController : DefaultController
    {
        public PrivacyController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [ServiceFilter(typeof(TrackPageView))]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Privacy Policy - " + _config.Title;
            ViewBag.Description = "Teknik privacy policy.";

            return View(new PrivacyViewModel());
        }
    }
}