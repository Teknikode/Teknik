using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.Abuse.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;

namespace Teknik.Areas.Abuse.Controllers
{
    [Authorize]
    [Area("Abuse")]
    public class AbuseController : DefaultController
    {
        public AbuseController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index()
        {
            ViewBag.Title = "Abuse Reporting";
            ViewBag.Description = "Methods for reporting abuse reports to Teknik Services.";

            return View(new AbuseViewModel());
        }
    }
}
