using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.ViewModels;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Filters;
using QRCoder;
using TwoStepsAuthenticator;
using System.Drawing;
using Teknik.Attributes;

namespace Teknik.Areas.Users.Controllers
{
    [TeknikAuthorize]
    public class UserController : DefaultController
    {
        private static readonly UsedCodesManager usedCodesManager = new UsedCodesManager();
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
                    model.SecuritySettings = user.SecuritySettings;
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
        public ActionResult Settings()
        {
            string username = User.Identity.Name;

            SettingsViewModel model = new SettingsViewModel();
            ViewBag.Title = "User Does Not Exist - " + Config.Title;
            ViewBag.Description = "The User does not exist";

            User user = UserHelper.GetUser(db, username);

            if (user != null)
            {
                Session["AuthenticatedUser"] = user;

                ViewBag.Title = "Settings - " + Config.Title;
                ViewBag.Description = "Your " + Config.Title + " Settings";

                model.UserID = user.UserId;
                model.Username = user.Username;
                model.TrustedDeviceCount = user.TrustedDevices.Count;
                model.AuthTokens = new List<AuthTokenViewModel>();
                foreach (AuthToken token in user.AuthTokens)
                {
                    AuthTokenViewModel tokenModel = new AuthTokenViewModel();
                    tokenModel.AuthTokenId = token.AuthTokenId;
                    tokenModel.Name = token.Name;
                    tokenModel.LastDateUsed = token.LastDateUsed;

                    model.AuthTokens.Add(tokenModel);
                }

                model.UserSettings = user.UserSettings;
                model.SecuritySettings = user.SecuritySettings;
                model.BlogSettings = user.BlogSettings;
                model.UploadSettings = user.UploadSettings;

                return View(model);
            }
            model.Error = true;
            return View(model);
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
                if (!string.IsNullOrEmpty(user.SecuritySettings.PGPSignature))
                {
                    return Content(user.SecuritySettings.PGPSignature, "text/plain");
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
                        // Perform transfer actions on the account
                        UserHelper.TransferUser(db, Config, user, model.Password);
                        user.LastSeen = DateTime.Now;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        // Let's double check their email and git accounts to make sure they exist
                        string email = UserHelper.GetUserEmailAddress(Config, username);
                        if (Config.EmailConfig.Enabled && !UserHelper.UserEmailExists(Config, email))
                        {
                            UserHelper.AddUserEmail(Config, email, model.Password);
                        }

                        if (Config.GitConfig.Enabled && !UserHelper.UserGitExists(Config, username))
                        {
                            UserHelper.AddUserGit(Config, username, model.Password);
                        }

                        bool twoFactor = false;
                        string returnUrl = model.ReturnUrl;
                        if (user.SecuritySettings.TwoFactorEnabled)
                        {
                            twoFactor = true;
                            // We need to check their device, and two factor them
                            if (user.SecuritySettings.AllowTrustedDevices)
                            {
                                // Check for the trusted device cookie
                                HttpCookie cookie = Request.Cookies[Constants.TRUSTEDDEVICECOOKIE + "_" + username];
                                if (cookie != null)
                                {
                                    string token = cookie.Value;
                                    if (user.TrustedDevices.Where(d => d.Token == token).FirstOrDefault() != null)
                                    {
                                        // The device token is attached to the user, let's let it slide
                                        twoFactor = false;
                                    }
                                }
                            }
                        }

                        if (twoFactor)
                        {
                            Session["AuthenticatedUser"] = user;
                            if (string.IsNullOrEmpty(model.ReturnUrl))
                                returnUrl = Request.UrlReferrer.AbsoluteUri.ToString();
                            returnUrl = Url.SubRouteUrl("user", "User.CheckAuthenticatorCode", new { returnUrl = returnUrl, rememberMe = model.RememberMe });
                            model.ReturnUrl = string.Empty;
                        }
                        else
                        {
                            returnUrl = Request.UrlReferrer.AbsoluteUri.ToString();
                            // They don't need two factor auth.
                            HttpCookie authcookie = UserHelper.CreateAuthCookie(user.Username, model.RememberMe, Request.Url.Host.GetDomain(), Request.IsLocal);
                            Response.Cookies.Add(authcookie);
                        }

                        if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return GenerateActionResult(new { result = returnUrl }, Redirect(returnUrl));
                        }
                        else
                        {
                            return Redirect(model.ReturnUrl);
                        }
                    }
                }
            }
            model.Error = true;
            model.ErrorMessage = "Invalid Username or Password.";

            return GenerateActionResult(new { error = model.ErrorMessage }, View("/Areas/User/Views/User/ViewLogin.cshtml", model));
        }

        public ActionResult Logout()
        {
            // Get cookie
            HttpCookie authCookie = UserHelper.CreateAuthCookie(User.Identity.Name, false, Request.Url.Host.GetDomain(), Request.IsLocal);

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
            model.Error = false;
            model.ErrorMessage = string.Empty;
            if (ModelState.IsValid)
            {
                if (Config.UserConfig.RegistrationEnabled)
                {
                    if (!model.Error && !UserHelper.ValidUsername(Config, model.Username))
                    {
                        model.Error = true;
                        model.ErrorMessage = "That username is not valid";
                    }
                    if (!model.Error && !UserHelper.UsernameAvailable(db, Config, model.Username))
                    {
                        model.Error = true;
                        model.ErrorMessage = "That username is not available";
                    }
                    if (!model.Error && model.Password != model.ConfirmPassword)
                    {
                        model.Error = true;
                        model.ErrorMessage = "Passwords must match";
                    }

                    // PGP Key valid?
                    if (!model.Error && !string.IsNullOrEmpty(model.PublicKey) && !PGP.IsPublicKey(model.PublicKey))
                    {
                        model.Error = true;
                        model.ErrorMessage = "Invalid PGP Public Key";
                    }

                    if (!model.Error)
                    {
                        try
                        {
                            User newUser = db.Users.Create();
                            newUser.JoinDate = DateTime.Now;
                            newUser.Username = model.Username;
                            newUser.UserSettings = new UserSettings();
                            newUser.SecuritySettings = new SecuritySettings();
                            newUser.BlogSettings = new BlogSettings();
                            newUser.UploadSettings = new UploadSettings();

                            if (!string.IsNullOrEmpty(model.PublicKey))
                                newUser.SecuritySettings.PGPSignature = model.PublicKey;
                            if (!string.IsNullOrEmpty(model.RecoveryEmail))
                                newUser.SecuritySettings.RecoveryEmail = model.RecoveryEmail;

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
                            model.Error = true;
                            model.ErrorMessage = ex.GetFullMessage(true);
                        }
                        if (!model.Error)
                        {
                            return Login(new LoginViewModel { Username = model.Username, Password = model.Password, RememberMe = false, ReturnUrl = model.ReturnUrl });
                        }
                    }
                }
                if (!model.Error)
                {
                    model.Error = true;
                    model.ErrorMessage = "User Registration is Disabled";
                }
            }
            return GenerateActionResult(new { error = model.ErrorMessage }, View("/Areas/User/Views/User/ViewRegistration.cshtml", model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string curPass, string newPass, string newPassConfirm, string pgpPublicKey, string recoveryEmail, bool allowTrustedDevices, bool twoFactorEnabled, string website, string quote, string about, string blogTitle, string blogDesc, bool saveKey, bool serverSideEncrypt)
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
                        user.SecuritySettings.PGPSignature = pgpPublicKey;

                        // Recovery Email
                        bool newRecovery = false;
                        if (recoveryEmail != user.SecuritySettings.RecoveryEmail)
                        {
                            newRecovery = true;
                            user.SecuritySettings.RecoveryEmail = recoveryEmail;
                            user.SecuritySettings.RecoveryVerified = false;
                        }

                        // Trusted Devices
                        user.SecuritySettings.AllowTrustedDevices = allowTrustedDevices;
                        if (!allowTrustedDevices)
                        {
                            // They turned it off, let's clear the trusted devices
                            user.TrustedDevices.Clear();
                            List<TrustedDevice> foundDevices = db.TrustedDevices.Where(d => d.UserId == user.UserId).ToList();
                            if (foundDevices != null)
                            {
                                foreach (TrustedDevice device in foundDevices)
                                {
                                    db.TrustedDevices.Remove(device);
                                }
                            }
                        }

                        // Two Factor Authentication
                        bool oldTwoFactor = user.SecuritySettings.TwoFactorEnabled;
                        user.SecuritySettings.TwoFactorEnabled = twoFactorEnabled;
                        string newKey = string.Empty;
                        if (!oldTwoFactor && twoFactorEnabled)
                        {
                            // They just enabled it, let's regen the key
                            newKey = Authenticator.GenerateKey();
                        }
                        else if (!twoFactorEnabled)
                        {
                            // remove the key when it's disabled
                            newKey = string.Empty;
                        }
                        else
                        {
                            // No change, let's use the old value
                            newKey = user.SecuritySettings.TwoFactorKey;
                        }
                        user.SecuritySettings.TwoFactorKey = newKey;

                        // Profile Info
                        user.UserSettings.Website = website;
                        user.UserSettings.Quote = quote;
                        user.UserSettings.About = about;

                        // Blogs
                        user.BlogSettings.Title = blogTitle;
                        user.BlogSettings.Description = blogDesc;

                        // Uploads
                        user.UploadSettings.SaveKey = saveKey;
                        user.UploadSettings.ServerSideEncrypt = serverSideEncrypt;

                        UserHelper.EditAccount(db, Config, user, changePass, newPass);

                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(recoveryEmail) && newRecovery)
                        {
                            string verifyCode = UserHelper.CreateRecoveryEmailVerification(db, Config, user);
                            string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = user.Username });
                            string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                            UserHelper.SendRecoveryEmailVerification(Config, user.Username, user.SecuritySettings.RecoveryEmail, resetUrl, verifyUrl);
                        }

                        if (!oldTwoFactor && twoFactorEnabled)
                        {
                            return Json(new { result = new { checkAuth = true, key = newKey, qrUrl = Url.SubRouteUrl("user", "User.Action", new { action = "GenerateAuthQrCode", key = newKey }) } });
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
                        if (!string.IsNullOrEmpty(user.SecuritySettings.RecoveryEmail))
                        {
                            if (!user.SecuritySettings.RecoveryVerified)
                            {
                                string verifyCode = UserHelper.CreateRecoveryEmailVerification(db, Config, user);
                                string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = user.Username });
                                string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                                UserHelper.SendRecoveryEmailVerification(Config, user.Username, user.SecuritySettings.RecoveryEmail, resetUrl, verifyUrl);
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
        [ValidateAntiForgeryToken]
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
                        if (!string.IsNullOrEmpty(user.SecuritySettings.RecoveryEmail) && user.SecuritySettings.RecoveryVerified)
                        {
                            string verifyCode = UserHelper.CreateResetPasswordVerification(db, Config, user);
                            string resetUrl = Url.SubRouteUrl("user", "User.VerifyResetPassword", new { Username = user.Username, Code = verifyCode });
                            UserHelper.SendResetPasswordVerification(Config, user.Username, user.SecuritySettings.RecoveryEmail, resetUrl);
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
                // The password reset code is valid, let's get their user account for this session
                User user = UserHelper.GetUser(db, username);
                Session["AuthenticatedUser"] = user;
                Session["AuthCode"] = code;
            }

            ResetPasswordVerificationViewModel model = new ResetPasswordVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ResetPasswordVerification.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult SetUserPassword(string password, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string code = Session["AuthCode"].ToString();
                    if (!string.IsNullOrEmpty(code))
                    {
                        User user = (User)Session["AuthenticatedUser"];
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

                            User newUser = UserHelper.GetUser(db, user.Username);
                            UserHelper.EditAccount(db, Config, newUser, true, password);

                            return Json(new { result = true });
                        }
                        return Json(new { error = "User does not exist" });
                    }
                    return Json(new { error = "Invalid Code" });
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.GetFullMessage(true) });
                }
            }
            return Json(new { error = "Unable to reset user password" });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmTwoFactorAuth(string returnUrl, bool rememberMe)
        {
            User user = (User)Session["AuthenticatedUser"];
            if (user != null)
            {
                ViewBag.Title = "Unknown Device - " + Config.Title;
                ViewBag.Description = "We do not recognize this device.";
                TwoFactorViewModel model = new TwoFactorViewModel();
                model.ReturnUrl = returnUrl;
                model.RememberMe = rememberMe;
                model.AllowTrustedDevice = user.SecuritySettings.AllowTrustedDevices;

                return View("/Areas/User/Views/User/TwoFactorCheck.cshtml", model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAuthenticatorCode(string code, string returnUrl, bool rememberMe, bool rememberDevice, string deviceName)
        {
            User user = (User)Session["AuthenticatedUser"];
            if (user != null)
            {
                if (user.SecuritySettings.TwoFactorEnabled)
                {
                    string key = user.SecuritySettings.TwoFactorKey;

                    TimeAuthenticator ta = new TimeAuthenticator(usedCodeManager: usedCodesManager);
                    bool isValid = ta.CheckCode(key, code, user);

                    if (isValid)
                    {
                        // the code was valid, let's log them in!
                        HttpCookie authcookie = UserHelper.CreateAuthCookie(user.Username, rememberMe, Request.Url.Host.GetDomain(), Request.IsLocal);
                        Response.Cookies.Add(authcookie);

                        if (user.SecuritySettings.AllowTrustedDevices && rememberDevice)
                        {
                            // They want to remember the device, and have allow trusted devices on
                            HttpCookie trustedDeviceCookie = UserHelper.CreateTrustedDeviceCookie(user.Username, Request.Url.Host.GetDomain(), Request.IsLocal);
                            Response.Cookies.Add(trustedDeviceCookie);

                            TrustedDevice device = new TrustedDevice();
                            device.UserId = user.UserId;
                            device.Name = (string.IsNullOrEmpty(deviceName)) ? "Unknown" : deviceName;
                            device.DateSeen = DateTime.Now;
                            device.Token = trustedDeviceCookie.Value;

                            // Add the token
                            db.TrustedDevices.Add(device);
                            db.SaveChanges();
                        }

                        if (string.IsNullOrEmpty(returnUrl))
                            returnUrl = Request.UrlReferrer.AbsoluteUri.ToString();
                        return Json(new { result = returnUrl });
                    }
                    return Json(new { error = "Invalid Authentication Code" });
                }
                return Json(new { error = "User does not have Two Factor Authentication enabled" });
            }
            return Json(new { error = "User does not exist" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyAuthenticatorCode(string code)
        {
            User user = UserHelper.GetUser(db, User.Identity.Name);
            if (user != null)
            {
                if (user.SecuritySettings.TwoFactorEnabled)
                {
                    string key = user.SecuritySettings.TwoFactorKey;

                    TimeAuthenticator ta = new TimeAuthenticator(usedCodeManager: usedCodesManager);
                    bool isValid = ta.CheckCode(key, code, user);

                    if (isValid)
                    {
                        return Json(new { result = true });
                    }
                    return Json(new { error = "Invalid Authentication Code" });
                }
                return Json(new { error = "User does not have Two Factor Authentication enabled" });
            }
            return Json(new { error = "User does not exist" });
        }

        [HttpGet]
        public ActionResult GenerateAuthQrCode(string key)
        {
            var ProvisionUrl = string.Format("otpauth://totp/{0}:{1}?secret={2}", Config.Title, User.Identity.Name, key);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(ProvisionUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return File(ByteHelper.ImageToByte(qrCodeImage), "image/png");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearTrustedDevices()
        {
            try
            {
                User user = UserHelper.GetUser(db, User.Identity.Name);
                if (user != null)
                {
                    if (user.SecuritySettings.AllowTrustedDevices)
                    {
                        // let's clear the trusted devices
                        user.TrustedDevices.Clear();
                        List<TrustedDevice> foundDevices = db.TrustedDevices.Where(d => d.UserId == user.UserId).ToList();
                        if (foundDevices != null)
                        {
                            foreach (TrustedDevice device in foundDevices)
                            {
                                db.TrustedDevices.Remove(device);
                            }
                        }
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json(new { result = true });
                    }
                    return Json(new { error = "User does not allow trusted devices" });
                }
                return Json(new { error = "User does not exist" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateToken(string name)
        {
            try
            {
                User user = UserHelper.GetUser(db, User.Identity.Name);
                if (user != null)
                {
                    string newTokenStr = UserHelper.GenerateAuthToken(db, user.Username);

                    if (!string.IsNullOrEmpty(newTokenStr))
                    {
                        AuthToken token = db.AuthTokens.Create();
                        token.UserId = user.UserId;
                        token.HashedToken = SHA256.Hash(newTokenStr);
                        token.Name = name;

                        db.AuthTokens.Add(token);
                        db.SaveChanges();

                        AuthTokenViewModel model = new AuthTokenViewModel();
                        model.AuthTokenId = token.AuthTokenId;
                        model.Name = token.Name;
                        model.LastDateUsed = token.LastDateUsed;

                        return Json(new { result = new { token = newTokenStr, html = PartialView("~/Areas/User/Views/User/AuthToken.cshtml", model).RenderToString() } });
                    }
                    return Json(new { error = "Unable to generate Auth Token" });
                }
                return Json(new { error = "User does not exist" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevokeAllTokens()
        {
            try
            {
                User user = UserHelper.GetUser(db, User.Identity.Name);
                if (user != null)
                {
                    user.AuthTokens.Clear();
                    List<AuthToken> foundTokens = db.AuthTokens.Where(d => d.UserId == user.UserId).ToList();
                    if (foundTokens != null)
                    {
                        foreach (AuthToken token in foundTokens)
                        {
                            db.AuthTokens.Remove(token);
                        }
                    }
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { result = true });
                }
                return Json(new { error = "User does not exist" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTokenName(int tokenId, string name)
        {
            try
            {
                User user = UserHelper.GetUser(db, User.Identity.Name);
                if (user != null)
                {
                    AuthToken foundToken = db.AuthTokens.Where(d => d.UserId == user.UserId && d.AuthTokenId == tokenId).FirstOrDefault();
                    if (foundToken != null)
                    {
                        foundToken.Name = name;
                        db.Entry(foundToken).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json(new { result = new { name = name } });
                    }
                    return Json(new { error = "Authentication Token does not exist" });
                }
                return Json(new { error = "User does not exist" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteToken(int tokenId)
        {
            try
            {
                User user = UserHelper.GetUser(db, User.Identity.Name);
                if (user != null)
                {
                    AuthToken foundToken = db.AuthTokens.Where(d => d.UserId == user.UserId && d.AuthTokenId == tokenId).FirstOrDefault();
                    if (foundToken != null)
                    {
                        db.AuthTokens.Remove(foundToken);
                        user.AuthTokens.Remove(foundToken);
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json(new { result = true });
                    }
                    return Json(new { error = "Authentication Token does not exist" });
                }
                return Json(new { error = "User does not exist" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }
    }
}