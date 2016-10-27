using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Admin.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Controllers;
using Teknik.Models;
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
        public ActionResult Search()
        {
            SearchViewModel model = new SearchViewModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult UserInfo(string username)
        {
            UserInfoViewModel model = new UserInfoViewModel();
            model.Username = username;
            return View(model);
        }

        [HttpPost]
        public ActionResult GetSearchResults(string query)
        {
            List<SearchResultViewModel> models = new List<SearchResultViewModel>();

            var results = db.Users.Where(u => u.Username.Contains(query)).ToList();
            if (results != null)
            {
                foreach (Users.Models.User user in results)
                {
                    SearchResultViewModel model = new SearchResultViewModel();
                    model.Username = user.Username;
                    if (Config.EmailConfig.Enabled)
                    {
                        model.Email = string.Format("{0}@{1}", user.Username, Config.EmailConfig.Domain);
                    }
                    model.JoinDate = user.JoinDate;
                    model.LastSeen = UserHelper.GetLastAccountActivity(db, Config, user);
                    models.Add(model);
                }
            }

            return PartialView("~/Areas/Admin/Views/Admin/SearchResults.cshtml", models);
        }
    }
}