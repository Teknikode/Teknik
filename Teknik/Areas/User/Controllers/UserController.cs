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
using Teknik.Filters;

namespace Teknik.Areas.Users.Controllers
{
    public class UserController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Profile/Profile
        [TrackPageView]
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
                User user = UserHelper.GetUser(db, username);

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

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Settings()
        {
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;

                SettingsViewModel model = new SettingsViewModel();
                ViewBag.Title = "User Does Not Exist - " + Config.Title;
                ViewBag.Description = "The User does not exist";

                User user = UserHelper.GetUser(db, username);

                if (user != null)
                {
                    ViewBag.Title = "Settings - " + Config.Title;
                    ViewBag.Description = "Your " + Config.Title + " Settings";

                    model.UserID = user.UserId;
                    model.Username = user.Username;
                    model.RecoveryEmail = user.RecoveryEmail;
                    model.RecoveryVerified = user.RecoveryVerified;

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
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult ViewRawPGP(string username)
        {
            ViewBag.Title = username + "'s Public Key - " + Config.Title;
            ViewBag.Description = "The PGP public key for " + username;

            User user = UserHelper.GetUser(db, username);
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
        [TrackPageView]
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
                User user = UserHelper.GetUser(db, username);
                if (user != null)
                {
                    bool userValid = UserHelper.UserPasswordCorrect(db, Config, user, model.Password);
                    if (userValid)
                    {
                        UserHelper.TransferUser(db, Config, user, model.Password);
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
        [TrackPageView]
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
                    if (!UserHelper.ValidUsername(Config, model.Username))
                    {
                        return Json(new { error = "That username is not valid" });
                    }
                    if (!UserHelper.UsernameAvailable(db, Config, model.Username))
                    {
                        return Json(new { error = "That username is not available" });
                    }
                    if (model.Password != model.ConfirmPassword)
                    {
                        return Json(new { error = "Passwords must match" });
                    }

                    // PGP Key valid?
                    if (!string.IsNullOrEmpty(model.PublicKey) && !PGP.IsPublicKey(model.PublicKey))
                    {
                        return Json(new { error = "Invalid PGP Public Key" });
                    }

                    try
                    {
                        User newUser = db.Users.Create();
                        newUser.JoinDate = DateTime.Now;
                        newUser.Username = model.Username;
                        if (!string.IsNullOrEmpty(model.RecoveryEmail))
                            newUser.RecoveryEmail = model.RecoveryEmail;
                        newUser.UserSettings = new UserSettings();
                        if (!string.IsNullOrEmpty(model.PublicKey))
                            newUser.UserSettings.PGPSignature = model.PublicKey;
                        newUser.BlogSettings = new BlogSettings();
                        newUser.UploadSettings = new UploadSettings();

                        UserHelper.AddAccount(db, Config, newUser, model.Password);
                        
                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(model.RecoveryEmail))
                        {
                            string verifyCode = UserHelper.CreateRecoveryEmailVerification(db, Config, newUser);
                            string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = model.Username });
                            string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                            UserHelper.SendRecoveryEmailVerification(Config, model.Username, model.RecoveryEmail, resetUrl, verifyUrl);
                        }
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
        public ActionResult Edit(string curPass, string newPass, string newPassConfirm, string pgpPublicKey, string recoveryEmail, string website, string quote, string about, string blogTitle, string blogDesc, bool saveKey, bool serverSideEncrypt)
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
                            if (!UserHelper.UserPasswordCorrect(db, Config, user, curPass))
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

                        bool newRecovery = false;
                        if (recoveryEmail != user.RecoveryEmail)
                        {
                            newRecovery = true;
                            user.RecoveryEmail = recoveryEmail;
                            user.RecoveryVerified = false;
                        }

                        user.UserSettings.Website = website;
                        user.UserSettings.Quote = quote;
                        user.UserSettings.About = about;

                        user.BlogSettings.Title = blogTitle;
                        user.BlogSettings.Description = blogDesc;

                        user.UploadSettings.SaveKey = saveKey;
                        user.UploadSettings.ServerSideEncrypt = serverSideEncrypt;
                        UserHelper.EditAccount(db, Config, user, changePass, newPass);

                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(recoveryEmail) && newRecovery)
                        {
                            string verifyCode = UserHelper.CreateRecoveryEmailVerification(db, Config, user);
                            string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = user.Username });
                            string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                            UserHelper.SendRecoveryEmailVerification(Config, user.Username, user.RecoveryEmail, resetUrl, verifyUrl);
                        }
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

        [HttpGet]
        public ActionResult VerifyRecoveryEmail(string code)
        {
            bool verified = true;
            if (string.IsNullOrEmpty(code))
                verified &= false;
            verified &= UserHelper.VerifyRecoveryEmail(db, Config, User.Identity.Name, code);

            RecoveryEmailVerificationViewModel model = new RecoveryEmailVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ViewRecoveryEmailVerification.cshtml", model);
        }

        [HttpPost]
        public ActionResult ResendVerifyRecoveryEmail()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(db, User.Identity.Name);
                    if (user != null)
                    {
                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(user.RecoveryEmail))
                        {
                            if (!user.RecoveryVerified)
                            {
                                string verifyCode = UserHelper.CreateRecoveryEmailVerification(db, Config, user);
                                string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = user.Username });
                                string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                                UserHelper.SendRecoveryEmailVerification(Config, user.Username, user.RecoveryEmail, resetUrl, verifyUrl);
                                return Json(new { result = true });
                            }
                            return Json(new { error = "The recovery email is already verified" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.GetFullMessage(true) });
                }
            }
            return Json(new { error = "Unable to resend verification" });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string username)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Username = username;

            return View("/Areas/User/Views/User/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SendResetPasswordVerification(string username)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(db, username);
                    if (user != null)
                    {
                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(user.RecoveryEmail) && user.RecoveryVerified)
                        {
                            string verifyCode = UserHelper.CreateResetPasswordVerification(db, Config, user);
                            string resetUrl = Url.SubRouteUrl("user", "User.VerifyResetPassword", new { Username = user.Username, Code = verifyCode });
                            UserHelper.SendResetPasswordVerification(Config, user.Username, user.RecoveryEmail, resetUrl);
                            return Json(new { result = true });
                        }
                        return Json(new { error = "The username doesn't have a recovery email specified" });
                    }
                    return Json(new { error = "The username is not valid" });
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.GetFullMessage(true) });
                }
            }
            return Json(new { error = "Unable to send reset link" });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyResetPassword(string username, string code)
        {
            bool verified = true;
            if (string.IsNullOrEmpty(code))
                verified &= false;
            verified &= UserHelper.VerifyResetPassword(db, Config, username, code);

            if (verified)
            {
                // The password reset code is valid, let's log them in
                User user = UserHelper.GetUser(db, username);
                user.LastSeen = DateTime.Now;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                HttpCookie authcookie = UserHelper.CreateAuthCookie(user.Username, false, Request.Url.Host.GetDomain(), Request.IsLocal);
                Response.Cookies.Add(authcookie);
            }

            ResetPasswordVerificationViewModel model = new ResetPasswordVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ResetPasswordVerification.cshtml", model);
        }

        [HttpPost]
        public ActionResult SetUserPassword(string password, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(db, User.Identity.Name);
                    if (user != null)
                    {
                        if (string.IsNullOrEmpty(password))
                        {
                            return Json(new { error = "Password must not be empty" });
                        }
                        if (password != confirmPassword)
                        {
                            return Json(new { error = "Passwords must match" });
                        }

                        UserHelper.EditAccount(db, Config, user, true, password);

                        return Json(new { result = true });
                    }
                    return Json(new { error = "User does not exist" });
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.GetFullMessage(true) });
                }
            }
            return Json(new { error = "Unable to reset user password" });
        }
    }
}