using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.Help.ViewModels;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;

namespace Teknik.Areas.Help.Controllers
{
    [Authorize]
    [Area("Help")]
    public class HelpController : DefaultController
    {
        public HelpController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Title = "Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View(model);
        }
        
        [AllowAnonymous]
        public IActionResult API(string version, string service)
        {
            HelpViewModel model = new HelpViewModel();
            if (string.IsNullOrEmpty(version) && string.IsNullOrEmpty(service))
            {
                ViewBag.Title = "API Help - " + _config.Title;
                return View("~/Areas/Help/Views/Help/API/API.cshtml", model);
            }
            else if(!string.IsNullOrEmpty(version) && !string.IsNullOrEmpty(service))
            {
                ViewBag.Title = service + " API " + version + " Help - " + _config.Title;
                return View("~/Areas/Help/Views/Help/API/" + version + "/" + service + ".cshtml", model);
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }
        
        [AllowAnonymous]
        public IActionResult Blog()
        {
            ViewBag.Title = "Blogging Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Blog.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult Git()
        {
            ViewBag.Title = "Git Service Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Git.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult IRC()
        {
            ViewBag.Title = "IRC Server Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/IRC.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult Mail()
        {
            ViewBag.Title = "Mail Server Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Mail.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult Markdown()
        {
            ViewBag.Title = "Markdown Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Markdown.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult Mumble()
        {
            ViewBag.Title = "Mumble Server Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Mumble.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult RSS()
        {
            ViewBag.Title = "RSS Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/RSS.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult Tools()
        {
            ViewBag.Title = "Tool Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Tools.cshtml", model);
        }
        
        [AllowAnonymous]
        public IActionResult Upload()
        {
            ViewBag.Title = "Upload Service Help - " + _config.Title;
            HelpViewModel model = new HelpViewModel();
            return View("~/Areas/Help/Views/Help/Upload.cshtml", model);
        }
    }
}