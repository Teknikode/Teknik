using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Configuration;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Controllers
{
    public class DefaultController : Controller
    {
        protected readonly ILogger<Logger> _logger;
        protected readonly Config _config;

        public DefaultController(ILogger<Logger> logger, Config config)
        {
            _logger = logger;
            _config = config;

            ViewBag.Title = string.Empty;
            ViewBag.Description = "Teknik Authentication Service";
        }

        // Get the Favicon
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        public IActionResult Favicon([FromServices] IHostingEnvironment env)
        {
            string imageFile = FileHelper.MapPath(env, Constants.FAVICON_PATH);
            FileStream fs = new FileStream(imageFile, FileMode.Open, FileAccess.Read);
            return File(fs, "image/x-icon");
        }

        // Get the Robots.txt
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Robots([FromServices] IHostingEnvironment env)
        {
            //string file = FileHelper.MapPath(env, Constants.ROBOTS_PATH);
            return File(Constants.ROBOTS_PATH, "text/plain");
        }

        protected IActionResult GenerateActionResult(object json)
        {
            return GenerateActionResult(json, View());
        }

        protected IActionResult GenerateActionResult(object json, IActionResult result)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(json);
            }
            return result;
        }
    }
}
