using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Teknik.Areas.Upload;
using Teknik.Areas.Paste;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using System.Text;
using MimeDetective;
using MimeDetective.Extensions;
using Teknik.Areas.Shortener.Models;
using nClam;
using Teknik.Filters;
using Teknik.Areas.API.V1.Models;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Teknik.Areas.Shortener;
using Teknik.Logging;
using Teknik.Areas.API.Controllers;

namespace Teknik.Areas.API.V1.Controllers
{
    public class APIv1Controller : APIController
    {
        public APIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
    }
}
