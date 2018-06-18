using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Logging;

namespace Teknik.Areas.Dev.Controllers
{
    [TeknikAuthorize]
    [Area("Dev")]
    public class DevController : DefaultController
    {
        public DevController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }
    }
}