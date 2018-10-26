using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teknik.Areas.Abuse.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;

namespace Teknik.Areas.Abuse.Controllers
{
    [Authorize]
    [Area("Abuse")]
    public class AbuseController : DefaultController
    {
        public AbuseController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Abuse Reporting - " + _config.Title;
            ViewBag.Description = "Methods for reporting abuse reports to Teknik Services.";

            return View(new AbuseViewModel());
        }
    }
}
