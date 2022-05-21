using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.FAQ.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;

namespace Teknik.Areas.FAQ.Controllers
{
    [Authorize]
    [Area("FAQ")]
    public class FAQController : DefaultController
    {
        public FAQController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index()
        {
            ViewBag.Title = "Frequently Asked Questions";
            FAQViewModel model = new FAQViewModel();
            return View(model);
        }
    }
}