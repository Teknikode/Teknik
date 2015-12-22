using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Profile.Models;
using Teknik.Areas.Profile.ViewModels;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Areas.Profile.Controllers
{
    public class ProfileController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Profile/Profile
        [AllowAnonymous]
        public ActionResult Index(string username)
        {
            ViewBag.Title = Config.Title + " - Profile";
            ViewBag.Message = "View Your Profile";

            return View(new ProfileViewModel());
        }

        [HttpGet]
        [AllowAnonymous]
        // GET: Profile
        public ActionResult Login(string ReturnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/Profile/Views/Profile/ViewLogin.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = model.Username;
                string password = SHA384.Hash(model.Username, model.Password);
                bool userValid = db.Users.Any(b => b.Username == username && b.HashedPassword == password);
                if (userValid)
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Json(new { result = "true" });
                    }
                    else
                    {
                        return Redirect(model.ReturnUrl);
                    }
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
                try
                {
                    // Add User
                    User newUser = db.Users.Create();
                    newUser.JoinDate = DateTime.Now;
                    newUser.Username = model.Username;
                    newUser.HashedPassword = SHA384.Hash(model.Username, model.Password);
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    // Generate blog for the user
                    var newBlog = db.Blogs.Create();
                    newBlog.UserId = db.Users.Where(u => u.Username == model.Username).Select(u => u.UserId).First();
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { error = "Unable to create the user." });
                }
                return Login(new LoginViewModel { Username = model.Username, Password = model.Password, RememberMe = false });
            }
            return Json(new { error = "You must include all fields." });
        }

    }
}