using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Admin.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Models;
using Teknik.Utilities;
using Teknik.ViewModels;

namespace Teknik.Areas.Admin.Controllers
{
    [TeknikAuthorize(Roles = "Admin")]
    public class AdminController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [HttpGet]
        public ActionResult Dashboard()
        {
            DashboardViewModel model = new DashboardViewModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult UserSearch()
        {
            UserSearchViewModel model = new UserSearchViewModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult UserInfo(string username)
        {
            if (UserHelper.UserExists(db, username))
            {
                User user = UserHelper.GetUser(db, username);
                UserInfoViewModel model = new UserInfoViewModel();
                model.Username = user.Username;
                model.AccountType = user.AccountType;
                model.AccountStatus = user.AccountStatus;
                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpGet]
        public ActionResult UploadSearch()
        {
            UploadSearchViewModel model = new UploadSearchViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult GetUserSearchResults(string query)
        {
            List<UserResultViewModel> models = new List<UserResultViewModel>();

            var results = db.Users.Where(u => u.Username.Contains(query)).ToList();
            if (results != null)
            {
                foreach (Users.Models.User user in results)
                {
                    try
                    {
                        UserResultViewModel model = new UserResultViewModel();
                        model.Username = user.Username;
                        if (Config.EmailConfig.Enabled)
                        {
                            model.Email = string.Format("{0}@{1}", user.Username, Config.EmailConfig.Domain);
                        }
                        model.JoinDate = user.JoinDate;
                        model.LastSeen = UserHelper.GetLastAccountActivity(db, Config, user);
                        models.Add(model);
                    }
                    catch (Exception ex)
                    {
                        // Skip this result
                    }
                }
            }

            return PartialView("~/Areas/Admin/Views/Admin/UserResults.cshtml", models);
        }

        [HttpPost]
        public ActionResult GetUploadSearchResults(string url)
        {
            Upload.Models.Upload foundUpload = db.Uploads.Where(u => u.Url == url).FirstOrDefault();
            if (foundUpload != null)
            {
                UploadResultViewModel model = new UploadResultViewModel();

                model.Url = foundUpload.Url;
                model.ContentType = foundUpload.ContentType;
                model.ContentLength = foundUpload.ContentLength;
                model.DateUploaded = foundUpload.DateUploaded;
                model.Downloads = foundUpload.Downloads;
                model.DeleteKey = foundUpload.DeleteKey;

                return PartialView("~/Areas/Admin/Views/Admin/UploadResult.cshtml", model);
            }
            return Json(new { error = new { message = "Upload does not exist" } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserAccountType(string username, AccountType accountType)
        {
            if (UserHelper.UserExists(db, username))
            {
                // Edit the user's account type
                UserHelper.EditAccountType(db, Config, username, accountType);
                return Json(new { result = new { success = true } });
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUserAccountStatus(string username, AccountStatus accountStatus)
        {
            if (UserHelper.UserExists(db, username))
            {
                // Edit the user's account type
                UserHelper.EditAccountStatus(db, Config, username, accountStatus);
                return Json(new { result = new { success = true } });
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }
    }
}
