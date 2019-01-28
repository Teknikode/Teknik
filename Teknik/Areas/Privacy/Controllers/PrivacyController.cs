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
    [Authorize]
    [Area("Privacy")]
    public class PrivacyController : DefaultController
    {
        public PrivacyController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Privacy Policy";
            ViewBag.Description = "Teknik privacy policy.";

            return View(new PrivacyViewModel());
        }
    }
}