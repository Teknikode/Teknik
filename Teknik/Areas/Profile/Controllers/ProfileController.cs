using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            if (string.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }

            ProfileViewModel model = null;
            ViewBag.Title = "User Does Not Exist - " + Config.Title;
            ViewBag.Message = "The User does not exist";

            User user = db.Users.Where(u => u.Username == username).First();

            if (user != null)
            {
                ViewBag.Title = username + "'s Profile - " + Config.Title;
                ViewBag.Message = "Viewing " + username + "'s Profile";

                model = new ProfileViewModel();
                model.UserID = user.UserId;
                model.Username = user.Username;
                model.Email = string.Format("{0}@{1}", user.Username, Config.Host);
                model.JoinDate = user.JoinDate;
                model.LastSeen = user.LastSeen;
                model.About = user.About;
                model.Website = user.Website;
                model.Quote = user.Quote;

                // fill in Blog details
                Blog.Models.Blog blog = db.Blogs.Where(b => b.UserId == user.UserId && b.BlogId != Constants.SERVERBLOGID).First();
                if (blog != null)
                {
                    model.BlogTitle = blog.Title;
                    model.BlogDescription = blog.Description;
                }
            }
            return View(model);
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
            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }

        [HttpGet]
        [AllowAnonymous]
        // GET: Profile
        public ActionResult Register(string ReturnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/Profile/Views/Profile/ViewRegistration.cshtml", model);
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
                return Login(new LoginViewModel { Username = model.Username, Password = model.Password, RememberMe = false, ReturnUrl = model.ReturnUrl });
            }
            return Json(new { error = "You must include all fields." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string curPass, string newPass, string newPassConfirm, string website, string quote, string about, string blogTitle, string blogDesc)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.Where(u => u.Username == User.Identity.Name).First();
                if (user != null)
                {
                    Blog.Models.Blog blog = db.Blogs.Where(b => b.UserId == user.UserId && b.BlogId != Constants.SERVERBLOGID).First();
                    if (blog != null)
                    {
                        // Changing Password?
                        if (!string.IsNullOrEmpty(curPass) && (!string.IsNullOrEmpty(newPass) || !string.IsNullOrEmpty(newPassConfirm)))
                        {
                            // Old Password Valid?
                            if (SHA384.Hash(User.Identity.Name, curPass) != user.HashedPassword)
                            {
                                return Json(new { error = "Invalid Original Password." });
                            }
                            // The New Password Match?
                            if (newPass != newPassConfirm)
                            {
                                return Json(new { error = "New Password Must Match." });
                            }
                            user.HashedPassword = SHA384.Hash(User.Identity.Name, newPass);
                        }
                        user.Website = website;
                        user.Quote = quote;
                        user.About = about;

                        blog.Title = blogTitle;
                        blog.Description = blogDesc;

                        db.Entry(blog).State = EntityState.Modified;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                }
            }
            return Json(new { error = "Unable to save profile." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int postID)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.Where(u => u.Username == User.Identity.Name).First();
                if (user != null)
                {
                    db.Users.Remove(user);
                    db.SaveChanges();
                    return Redirect(Url.SubRouteUrl("www", "Home.Index"));
                }
            }
            return Json(new { error = "Unable to delete user." });
        }
    }
}