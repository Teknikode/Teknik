using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Areas.API.Controllers;

namespace Teknik.Areas.API.V1.Controllers
{
    public class APIv1Controller : APIController
    {
        public APIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
    }
}
