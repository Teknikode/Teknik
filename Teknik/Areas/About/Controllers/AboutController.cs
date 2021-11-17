using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.About.ViewModels;
using Teknik.Areas.Billing.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.BillingCore;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.About.Controllers
{
    [Authorize]
    [Area("About")]
    public class AboutController : DefaultController
    {
        public AboutController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index([FromServices] Config config)
        {
            ViewBag.Title = "About";
            ViewBag.Description = "What is Teknik?";
            var vm = new AboutViewModel();

            return View(vm);
        }
    }
}