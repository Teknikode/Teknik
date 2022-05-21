using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Logging;

namespace Teknik.Areas.API.V1.Controllers
{
    [Authorize(Policy = "AnyAPI")]
    public class AccountAPIv1Controller : APIv1Controller
    {
        public AccountAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [HttpGet]
        [TrackPageView]
        public IActionResult GetClaims()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}