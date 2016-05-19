using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Error.Controllers;
using Teknik.Areas.Error.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.ViewModels;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;
using Teknik.ViewModels;
using System.Windows;
using System.Net;
using Teknik.Areas.Users.Utility;

namespace Teknik.Areas.Users.Controllers
{
    public class UserController : DefaultController
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

            ProfileViewModel model = new ProfileViewModel();
            ViewBag.Title = "User Does Not Exist - " + Config.Title;
            ViewBag.Description = "The User does not exist";

            try
            {
                User user = db.Users.Where(u => u.Username == username).FirstOrDefault();

                if (user != null)
                {
                    ViewBag.Title = username + "'s Profile - " + Config.Title;
                    ViewBag.Description = "Viewing " + username + "'s Profile";

                    model.UserID = user.UserId;
                    model.Username = user.Username;
                    if (Config.EmailConfig.Enabled)
                    {
                        model.Email = string.Format("{0}@{1}", user.Username, Config.EmailConfig.Domain);
                    }
                    model.JoinDate = user.JoinDate;
                    model.LastSeen = UserHelper.GetLastAccountActivity(db, Config, user);

                    model.UserSettings = user.UserSettings;
                    model.BlogSettings = user.BlogSettings;
                    model.UploadSettings = user.UploadSettings;

                    model.Uploads = db.Uploads.Where(u => u.UserId == user.UserId).OrderByDescending(u => u.DateUploaded).ToList();

                    model.Pastes = db.Pastes.Where(u => u.UserId == user.UserId).OrderByDescending(u => u.DatePosted).ToList();

                    model.ShortenedUrls = db.ShortenedUrls.Where(s => s.UserId == user.UserId).OrderByDescending(s => s.DateAdded).ToList();

                    return View(model);
                }
                model.Error = true;
                model.ErrorMessage = "The user does not exist";
            }
            catch (Exception ex)
            {
                model.Error = true;
                model.ErrorMessage = ex.GetFullMessage(true);
            }
            return View(model);
        }
        
        [AllowAnonymous]
        public ActionResult Settings()
        {
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;

                SettingsViewModel model = new SettingsViewModel();
                ViewBag.Title = "User Does Not Exist - " + Config.Title;
                ViewBag.Description = "The User does not exist";

                User user = db.Users.Where(u => u.Username == username).FirstOrDefault();

                if (user != null)
                {
                    ViewBag.Title = "Settings - " + Config.Title;
                    ViewBag.Description = "Your " + Config.Title + " Settings";

                    model.UserID = user.UserId;
                    model.Username = user.Username;

                    model.UserSettings = user.UserSettings;
                    model.BlogSettings = user.BlogSettings;
                    model.UploadSettings = user.UploadSettings;

                    return View(model);
                }
                model.Error = true;
                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ViewRawPGP(string username)
        {
            ViewBag.Title = username + "'s Public Key - " + Config.Title;
            ViewBag.Description = "The PGP public key for " + username;

            User user = db.Users.Where(u => u.Username == username).FirstOrDefault();
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.UserSettings.PGPSignature))
                {
                    return Content(user.UserSettings.PGPSignature, "text/plain");
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/User/Views/User/ViewLogin.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login([Bind(Prefix = "Login")]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = model.Username;
                string password = SHA384.Hash(model.Username, model.Password);
                User user = db.Users.Where(b => b.Username == username).FirstOrDefault();
                if (user != null)
                {
                    if (user.TransferAccount)
                    {
                        password = SHA256.Hash(model.Password, Config.Salt1, Config.Salt2);
                    }
                    bool userValid = db.Users.Any(b => b.Username == username && b.HashedPassword == password);
                    if (userValid)
                    {
                        if (user.TransferAccount)
                        {
                            user.HashedPassword = SHA384.Hash(model.Username, model.Password);
                            user.TransferAccount = false;
                        }
                        user.LastSeen = DateTime.Now;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                        HttpCookie authcookie = UserHelper.CreateAuthCookie(model.Username, model.RememberMe, Request.Url.Host.GetDomain(), Request.IsLocal);
                        Response.Cookies.Add(authcookie);

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
            }
            return Json(new { error = "Invalid Username or Password." });
        }

        public ActionResult Logout()
        {
            // Get cookie
            HttpCookie authCookie = Utility.UserHelper.CreateAuthCookie(User.Identity.Name, false, Request.Url.Host.GetDomain(), Request.IsLocal);

            // Signout
            FormsAuthentication.SignOut();
            Session.Abandon();

            // Destroy Cookies
            authCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(authCookie);

            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register(string ReturnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/User/Views/User/ViewRegistration.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register([Bind(Prefix="Register")]RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Config.UserConfig.RegistrationEnabled)
                {
                    if (!UserHelper.UsernameAvailable(db, Config, model.Username))
                    {
                        return Json(new { error = "That username is not available." });
                    }
                    if (model.Password != model.ConfirmPassword)
                    {
                        return Json(new { error = "Passwords must match." });
                    }

                    try
                    {
                        User newUser = db.Users.Create();
                        newUser.JoinDate = DateTime.Now;
                        newUser.Username = model.Username;
                        newUser.UserSettings = new UserSettings();
                        newUser.BlogSettings = new BlogSettings();
                        newUser.UploadSettings = new UploadSettings();

                        UserHelper.AddAccount(db, Config, newUser, model.Password);
                    }
                    catch (Exception ex)
                    {
                        return Json(new { error = ex.GetFullMessage(true) });
                    }
                    return Login(new LoginViewModel { Username = model.Username, Password = model.Password, RememberMe = false, ReturnUrl = model.ReturnUrl });
                }
                return Json(new { error = "User Registration is Disabled" });
            }
            return Json(new { error = "You must include all fields." });
        }

        [HttpPost]
        public ActionResult Edit(string curPass, string newPass, string newPassConfirm, string pgpPublicKey, string website, string quote, string about, string blogTitle, string blogDesc, bool saveKey, bool serverSideEncrypt)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(db, User.Identity.Name);
                    if (user != null)
                    {
                        bool changePass = false;
                        string email = string.Format("{0}@{1}", User.Identity.Name, Config.EmailConfig.Domain);
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
                            changePass = true;
                        }

                        // PGP Key valid?
                        if (!string.IsNullOrEmpty(pgpPublicKey) && !PGP.IsPublicKey(pgpPublicKey))
                        {
                            return Json(new { error = "Invalid PGP Public Key" });
                        }
                        user.UserSettings.PGPSignature = pgpPublicKey;

                        user.UserSettings.Website = website;
                        user.UserSettings.Quote = quote;
                        user.UserSettings.About = about;

                        user.BlogSettings.Title = blogTitle;
                        user.BlogSettings.Description = blogDesc;

                        user.UploadSettings.SaveKey = saveKey;
                        user.UploadSettings.ServerSideEncrypt = serverSideEncrypt;
                        UserHelper.EditAccount(db, Config, user, changePass, newPass);
                        return Json(new { result = true });
                    }
                    return Json(new { error = "User does not exist" });
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.GetFullMessage(true) });
                }
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public ActionResult Delete()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(db, User.Identity.Name);
                    if (user != null)
                    {
                        UserHelper.DeleteAccount(db, Config, user);
                        // Sign Out
                        Logout();
                        return Json(new { result = true });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.GetFullMessage(true) });
                }
            }
            return Json(new { error = "Unable to delete user" });
        }
    }
}