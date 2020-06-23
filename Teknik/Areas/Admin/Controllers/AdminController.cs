using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Admin.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Utilities;
using Teknik.ViewModels;
using Teknik.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Teknik.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminController : DefaultController
    {
        public AdminController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base (logger, config, dbContext) { }

        [HttpGet]
        [TrackPageView]
        public IActionResult Dashboard()
        {
            DashboardViewModel model = new DashboardViewModel();
            return View(model);
        }

        [HttpGet]
        [TrackPageView]
        public IActionResult UserSearch()
        {
            UserSearchViewModel model = new UserSearchViewModel();
            return View(model);
        }

        [HttpGet]
        [TrackPageView]
        public async Task<IActionResult> UserInfo(string username)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                User user = UserHelper.GetUser(_dbContext, username);
                UserInfoViewModel model = new UserInfoViewModel();
                model.Username = user.Username;

                // Get Identity User Info
                var info = await IdentityHelper.GetIdentityUserInfo(_config, username);
                if (info.AccountType.HasValue)
                    model.AccountType = info.AccountType.Value;
                if (info.AccountStatus.HasValue)
                    model.AccountStatus = info.AccountStatus.Value;
                return View(model);
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [TrackPageView]
        public IActionResult UploadSearch()
        {
            UploadSearchViewModel model = new UploadSearchViewModel();
            return View(model);
        }

        [HttpGet]
        [TrackPageView]
        public IActionResult PasteSearch()
        {
            PasteSearchViewModel model = new PasteSearchViewModel();
            return View(model);
        }

        [HttpGet]
        [TrackPageView]
        public IActionResult ShoretenedUrlSearch()
        {
            UploadSearchViewModel model = new UploadSearchViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetUserSearchResults(string query, [FromServices] ICompositeViewEngine viewEngine)
        {
            List<UserResultViewModel> models = new List<UserResultViewModel>();

            var results = _dbContext.Users.Where(u => u.Username.Contains(query)).ToList();
            if (results != null)
            {
                foreach (User user in results)
                {
                    try
                    {
                        UserResultViewModel model = new UserResultViewModel();
                        model.Username = user.Username;
                        if (_config.EmailConfig.Enabled)
                        {
                            model.Email = string.Format("{0}@{1}", user.Username, _config.EmailConfig.Domain);
                        }
                        var info = await IdentityHelper.GetIdentityUserInfo(_config, user.Username);
                        if (info.CreationDate.HasValue)
                            model.JoinDate = info.CreationDate.Value;

                        model.LastSeen = await UserHelper.GetLastAccountActivity(_dbContext, _config, user.Username);
                        models.Add(model);
                    }
                    catch (Exception)
                    {
                        // Skip this result
                    }
                }
            }

            string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/Admin/Views/Admin/UserResults.cshtml", models);

            return Json(new { result = new { html = renderedView } });
        }

        [HttpPost]
        public async Task<IActionResult> GetUploadSearchResults(string url, [FromServices] ICompositeViewEngine viewEngine)
        {
            Upload.Models.Upload foundUpload = _dbContext.Uploads.Where(u => u.Url == url).FirstOrDefault();
            if (foundUpload != null)
            {
                UploadResultViewModel model = new UploadResultViewModel();

                model.Url = foundUpload.Url;
                model.ContentType = foundUpload.ContentType;
                model.ContentLength = foundUpload.ContentLength;
                model.DateUploaded = foundUpload.DateUploaded;
                model.Downloads = foundUpload.Downloads;
                model.DeleteKey = foundUpload.DeleteKey;
                model.Username = foundUpload.User?.Username;

                string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/Admin/Views/Admin/UploadResult.cshtml", model);

                return Json(new { result = new { html = renderedView } });
            }
            return Json(new { error = new { message = "Upload does not exist" } });
        }

        [HttpPost]
        public async Task<IActionResult> GetPasteSearchResults(string url, [FromServices] ICompositeViewEngine viewEngine)
        {
            Paste.Models.Paste foundPaste = _dbContext.Pastes.Where(u => u.Url == url).FirstOrDefault();
            if (foundPaste != null)
            {
                PasteResultViewModel model = new PasteResultViewModel();

                model.Url = foundPaste.Url;
                model.DatePosted = foundPaste.DatePosted;
                model.Views = foundPaste.Views;
                model.DeleteKey = foundPaste.DeleteKey;
                model.Username = foundPaste.User?.Username;

                string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/Admin/Views/Admin/PasteResult.cshtml", model);

                return Json(new { result = new { html = renderedView } });
            }
            return Json(new { error = new { message = "Paste does not exist" } });
        }

        [HttpPost]
        public async Task<IActionResult> GetShortenedUrlSearchResults(string url, [FromServices] ICompositeViewEngine viewEngine)
        {
            Shortener.Models.ShortenedUrl foundUrl = _dbContext.ShortenedUrls.Where(u => u.ShortUrl == url).FirstOrDefault();
            if (foundUrl != null)
            {
                ShortenedUrlResultViewModel model = new ShortenedUrlResultViewModel();

                model.OriginalUrl = foundUrl.OriginalUrl;
                model.ShortenedUrl = foundUrl.ShortUrl;
                model.CreationDate = foundUrl.DateAdded;
                model.Views = foundUrl.Views;
                model.Username = foundUrl.User?.Username;

                string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/Admin/Views/Admin/ShortenedUrlResult.cshtml", model);

                return Json(new { result = new { html = renderedView } });
            }
            return Json(new { error = new { message = "Shortened Url does not exist" } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserAccountType(string username, AccountType accountType)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                // Edit the user's account type
                await UserHelper.EditAccountType(_dbContext, _config, username, accountType);
                return Json(new { result = new { success = true } });
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserAccountStatus(string username, AccountStatus accountStatus)
        {
            if (UserHelper.UserExists(_dbContext, username))
            {
                // Edit the user's account type
                await UserHelper.EditAccountStatus(_dbContext, _config, username, accountStatus);
                return Json(new { result = new { success = true } });
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateInviteCode(string username)
        {
            InviteCode inviteCode = new InviteCode();
            inviteCode.Active = true;
            inviteCode.Code = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(username))
            {
                if (!UserHelper.UserExists(_dbContext, username))
                {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                User user = UserHelper.GetUser(_dbContext, username);
                inviteCode.Owner = user;
            }
            _dbContext.InviteCodes.Add(inviteCode);
            _dbContext.SaveChanges();

            return Json(new { result = new { code = inviteCode.Code } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string username)
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, username);
                if (user != null)
                {
                    await UserHelper.DeleteAccount(_dbContext, _config, user);
                    return Json(new { result = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
            return Json(new { error = "Unable to delete user" });
        }
    }
}
