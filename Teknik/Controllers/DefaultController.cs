using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.Controllers
{
    [Area("Default")]
    [CORSActionFilter]
    public class DefaultController : Controller
    {
        protected string Subdomain
        {
            get { return (string)this.ControllerContext.RouteData.Values["sub"]; }
        }

        protected readonly ILogger<Logger> _logger;
        protected readonly Config _config;
        protected readonly TeknikEntities _dbContext;

        public DefaultController(ILogger<Logger> logger, Config config, TeknikEntities dbContext)
        {
            _logger = logger;
            _config = config;
            _dbContext = dbContext;

            ViewBag.Title = string.Empty;
            ViewBag.Description = _config.Description;
        }

        // Get the Favicon
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        [IgnoreAntiforgeryToken]
        public IActionResult Favicon([FromServices] IWebHostEnvironment env)
        {
            string imageFile = FileHelper.MapPath(env, Constants.FAVICON_PATH);
            FileStream fs = new FileStream(imageFile, FileMode.Open, FileAccess.Read);
            return File(fs, "image/x-icon");
        }

        // Get the Logo
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        [IgnoreAntiforgeryToken]
        public IActionResult Logo([FromServices] IWebHostEnvironment env)
        {
            string imageFile = FileHelper.MapPath(env, Constants.LOGO_PATH);
            FileStream fs = new FileStream(imageFile, FileMode.Open, FileAccess.Read);
            return File(fs, "image/svg+xml");
        }

        // Get the Robots.txt
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Robots([FromServices] IWebHostEnvironment env)
        {
            string dataDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            string file = Path.Combine(dataDir, Constants.ROBOTS_PATH);
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            return File(fs, "text/plain");
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

        protected async Task<string> RenderPartialViewToString(ICompositeViewEngine viewEngine, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                string path = (new Uri(Assembly.GetExecutingAssembly().Location)).AbsolutePath;
                ViewEngineResult viewResult =
                    viewEngine.GetView(path, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
