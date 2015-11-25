using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teknik.Areas.Profile.ViewModels;
using Teknik.Controllers;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Profile.Controllers
{
    public class ProfileController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Profile/Profile
        public ActionResult Index()
        {
            ViewBag.Title = Config.Title + " - Profile";
            ViewBag.Message = "View Your Profile";

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        // GET: Profile
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsValid())
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Json(new { result = "true" });
                }
            }
            return Json(new { error = "Invalid User name or Password." });
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", new { Area = "Home" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var foundUser = db.Users.Where(b => b.Username == model.Username);
                if (foundUser.Any())
                {
                    return Json(new { error = "That username already exists." });
                }
                if (model.Password != model.ConfirmPassword)
                {
                    return Json(new { error = "Passwords must match." });
                }
                if (model.Insert())
                {
                    return RedirectToAction("Login", "Profile", new LoginViewModel { Username = model.Username, Password = model.Password });
                }
                return Json(new { error = "You must include all fields." });
            }
            return Json(new { error = "You must include all fields." });
        }

    }
}