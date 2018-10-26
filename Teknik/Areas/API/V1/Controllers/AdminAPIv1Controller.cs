using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.API.Controllers;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;

namespace Teknik.Areas.API.V1.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin", Policy = "AnyAPI")]
    public class AdminAPIv1Controller : APIv1Controller
    {
        public AdminAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
    }
}