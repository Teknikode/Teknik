using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;

namespace Teknik.Areas.API.V1.Controllers
{
    [Authorize(Policy = "AnyAPI")]
    public class AccountAPIv1Controller : APIv1Controller
    {
        public AccountAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [HttpGet]
        [ServiceFilter(typeof(TrackPageView))]
        public IActionResult GetClaims()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}