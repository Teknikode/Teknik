using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Areas.API.Controllers;
using Teknik.Areas.API.V1.Models;
using Teknik.Areas.Paste;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Logging;
using Teknik.Utilities;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.API.V1.Controllers
{
    [Authorize(Policy = "WriteAPI")]
    public class PasteAPIv1Controller : APIv1Controller
    {
        public PasteAPIv1Controller(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }

        [HttpPost]
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Paste(PasteAPIv1Model model)
        {
            try
            {
                if (model != null && model.code != null)
                {
                    Paste.Models.Paste paste = PasteHelper.CreatePaste(_config, _dbContext, model.code, model.title, model.syntax, model.expireUnit, model.expireLength, model.password);

                    // Associate this with the user if they are logged in
                    if (User.Identity.IsAuthenticated)
                    {
                        User foundUser = UserHelper.GetUser(_dbContext, User.Identity.Name);
                        if (foundUser != null)
                        {
                            paste.UserId = foundUser.UserId;
                        }
                    }

                    _dbContext.Pastes.Add(paste);
                    _dbContext.SaveChanges();

                    return Json(new
                    {
                        result = new
                        {
                            id = paste.Url,
                            url = Url.SubRouteUrl("p", "Paste.View", new { type = "Full", url = paste.Url, password = model.password }),
                            title = paste.Title,
                            syntax = paste.Syntax,
                            expiration = paste.ExpireDate,
                            password = model.password
                        }
                    });
                }
                return Json(new { error = new { message = "Invalid Paste Request" } });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = "Exception: " + ex.Message } });
            }
        }
    }
}