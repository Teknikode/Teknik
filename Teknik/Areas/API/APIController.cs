using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Teknik.Areas.Upload;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Mvc;
using Teknik.Logging;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.API.Controllers
{
    [Area("API")]
    public class APIController : DefaultController
    {
        public APIController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("help", "Help.API"));
        }
    }
}