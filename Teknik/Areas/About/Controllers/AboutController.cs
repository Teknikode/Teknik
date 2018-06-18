using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.About.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;

namespace Teknik.Areas.About.Controllers
{
    [TeknikAuthorize]
    [Area("About")]
    public class AboutController : DefaultController
    {
        public AboutController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        [ServiceFilter(typeof(TrackPageView))]
        public IActionResult Index([FromServices] Config config)
        {
            ViewBag.Title = "About - " + config.Title;
            ViewBag.Description = "What is Teknik?";

            return View(new AboutViewModel());
        }
    }
}