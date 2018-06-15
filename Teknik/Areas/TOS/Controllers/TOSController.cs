using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.TOS.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;

namespace Teknik.Areas.TOS.Controllers
{
    [TeknikAuthorize]
    [Area("TOS")]
    public class TOSController : DefaultController
    {
        public TOSController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [TrackPageView]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Terms of Service - " + _config.Title;
            ViewBag.Description = "Teknik Terms of Service.";

            return View(new TOSViewModel());
        }
    }
}