using System;
using System.Collections.Generic;
using System.Linq;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.ViewModels;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Areas.Users.Utility;
using Teknik.Filters;
using QRCoder;
using TwoStepsAuthenticator;
using Teknik.Attributes;
using Teknik.Utilities.Cryptography;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;
using Teknik.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Teknik.Areas.Accounts.Controllers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;

namespace Teknik.Areas.Users.Controllers
{
    [TeknikAuthorize]
    [Area("User")]
    public class UserController : DefaultController
    {
        private static readonly UsedCodesManager usedCodesManager = new UsedCodesManager();
        private const string _AuthSessionKey = "AuthenticatedUser";

        private readonly SignInManager<User> _signInManager;
        private readonly IEventService _events;

        public UserController(ILogger<Logger> logger, Config config, TeknikEntities dbContext, SignInManager<User> signInManager, IEventService eventService) : base(logger, config, dbContext)
        {
            _signInManager = signInManager;
            _events = eventService;
        }
        
        [AllowAnonymous]
        public IActionResult GetPremium()
        {
            ViewBag.Title = "Get a Premium Account - " + _config.Title;

            GetPremiumViewModel model = new GetPremiumViewModel();

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string inviteCode, string ReturnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();
            model.InviteCode = inviteCode;
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/User/Views/User/ViewRegistration.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([Bind(Prefix = "Register")]RegisterViewModel model)
        {
            model.Error = false;
            model.ErrorMessage = string.Empty;
            if (ModelState.IsValid)
            {
                if (_config.UserConfig.RegistrationEnabled)
                {
                    if (!model.Error && !UserHelper.ValidUsername(_config, model.Username))
                    {
                        model.Error = true;
                        model.ErrorMessage = "That username is not valid";
                    }
                    if (!model.Error && !UserHelper.UsernameAvailable(_dbContext, _config, model.Username))
                    {
                        model.Error = true;
                        model.ErrorMessage = "That username is not available";
                    }
                    if (!model.Error && model.Password != model.ConfirmPassword)
                    {
                        model.Error = true;
                        model.ErrorMessage = "Passwords must match";
                    }

                    // Validate the Invite Code
                    if (!model.Error && _config.UserConfig.InviteCodeRequired && string.IsNullOrEmpty(model.InviteCode))
                    {
                        model.Error = true;
                        model.ErrorMessage = "An Invite Code is required to register";
                    }
                    if (!model.Error && !string.IsNullOrEmpty(model.InviteCode) && _dbContext.InviteCodes.Where(c => c.Code == model.InviteCode && c.Active && c.ClaimedUser == null).FirstOrDefault() == null)
                    {
                        model.Error = true;
                        model.ErrorMessage = "Invalid Invite Code";
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
                            User newUser = new User();
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

                            // if they provided an invite code, let's assign them to it
                            if (!string.IsNullOrEmpty(model.InviteCode))
                            {
                                InviteCode code = _dbContext.InviteCodes.Where(c => c.Code == model.InviteCode).FirstOrDefault();
                                _dbContext.Entry(code).State = EntityState.Modified;
                                _dbContext.SaveChanges();

                                newUser.ClaimedInviteCode = code;
                            }

                            UserHelper.AddAccount(_dbContext, _config, newUser, model.Password);

                            // If they have a recovery email, let's send a verification
                            if (!string.IsNullOrEmpty(model.RecoveryEmail))
                            {
                                string verifyCode = UserHelper.CreateRecoveryEmailVerification(_dbContext, _config, newUser);
                                string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = model.Username });
                                string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                                UserHelper.SendRecoveryEmailVerification(_config, model.Username, model.RecoveryEmail, resetUrl, verifyUrl);
                            }
                        }
                        catch (Exception ex)
                        {
                            model.Error = true;
                            model.ErrorMessage = ex.GetFullMessage(true);
                        }
                        if (!model.Error)
                        {
                            return Redirect(Url.SubRouteUrl("accounts", "Accounts.Login", new { ReturnUrl = model.ReturnUrl }));
                        }
                    }
                }
                if (!model.Error)
                {
                    model.Error = true;
                    model.ErrorMessage = "User Registration is Disabled";
                }
            }
            else
            {
                model.Error = true;
                model.ErrorMessage = "Missing Required Fields";
            }
            return GenerateActionResult(new { error = model.ErrorMessage }, View("/Areas/User/Views/User/ViewRegistration.cshtml", model));
        }

        // GET: Profile/Profile
        [AllowAnonymous]
        public IActionResult ViewProfile(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }

            ProfileViewModel model = new ProfileViewModel();
            ViewBag.Title = "User Does Not Exist - " + _config.Title;
            ViewBag.Description = "The User does not exist";

            try
            {
                User user = UserHelper.GetUser(_dbContext, username);

                if (user != null)
                {
                    ViewBag.Title = username + "'s Profile - " + _config.Title;
                    ViewBag.Description = "Viewing " + username + "'s Profile";

                    model.UserID = user.UserId;
                    model.Username = user.Username;
                    if (_config.EmailConfig.Enabled)
                    {
                        model.Email = string.Format("{0}@{1}", user.Username, _config.EmailConfig.Domain);
                    }
                    model.JoinDate = user.JoinDate;
                    model.LastSeen = UserHelper.GetLastAccountActivity(_dbContext, _config, user);
                    model.AccountType = user.AccountType;
                    model.AccountStatus = user.AccountStatus;

                    model.UserSettings = user.UserSettings;
                    model.SecuritySettings = user.SecuritySettings;
                    model.BlogSettings = user.BlogSettings;
                    model.UploadSettings = user.UploadSettings;

                    model.Uploads = _dbContext.Uploads.Where(u => u.UserId == user.UserId).OrderByDescending(u => u.DateUploaded).ToList();

                    model.Pastes = _dbContext.Pastes.Where(u => u.UserId == user.UserId).OrderByDescending(u => u.DatePosted).ToList();

                    model.ShortenedUrls = _dbContext.ShortenedUrls.Where(s => s.UserId == user.UserId).OrderByDescending(s => s.DateAdded).ToList();

                    model.Vaults = _dbContext.Vaults.Where(v => v.UserId == user.UserId).OrderByDescending(v => v.DateCreated).ToList();

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
        
        public IActionResult Settings()
        {
            return Redirect(Url.SubRouteUrl("user", "User.SecuritySettings"));
        }
        
        public IActionResult ProfileSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                HttpContext.Session.Set(_AuthSessionKey, user.Username);

                ViewBag.Title = "Profile Settings - " + _config.Title;
                ViewBag.Description = "Your " + _config.Title + " Profile Settings";

                ProfileSettingsViewModel model = new ProfileSettingsViewModel();
                model.Page = "Profile";
                model.UserID = user.UserId;
                model.Username = user.Username;
                model.About = user.UserSettings.About;
                model.Quote = user.UserSettings.Quote;
                model.Website = user.UserSettings.Website;

                return View("/Areas/User/Views/User/Settings/ProfileSettings.cshtml", model);
            }

            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }
        
        public IActionResult SecuritySettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                HttpContext.Session.Set(_AuthSessionKey, user.Username);

                ViewBag.Title = "Security Settings - " + _config.Title;
                ViewBag.Description = "Your " + _config.Title + " Security Settings";

                SecuritySettingsViewModel model = new SecuritySettingsViewModel();
                model.Page = "Security";
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

                model.PgpPublicKey = user.SecuritySettings.PGPSignature;
                model.RecoveryEmail = user.SecuritySettings.RecoveryEmail;
                model.RecoveryVerified = user.SecuritySettings.RecoveryVerified;
                model.AllowTrustedDevices = user.SecuritySettings.AllowTrustedDevices;
                model.TwoFactorEnabled = user.SecuritySettings.TwoFactorEnabled;
                model.TwoFactorKey = user.SecuritySettings.TwoFactorKey;

                return View("/Areas/User/Views/User/Settings/SecuritySettings.cshtml", model);
            }

            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }
        
        public IActionResult InviteSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                HttpContext.Session.Set(_AuthSessionKey, user.Username);

                ViewBag.Title = "Invite Settings - " + _config.Title;
                ViewBag.Description = "Your " + _config.Title + " Invite Settings";

                InviteSettingsViewModel model = new InviteSettingsViewModel();
                model.Page = "Invite";
                model.UserID = user.UserId;
                model.Username = user.Username;

                List<InviteCodeViewModel> availableCodes = new List<InviteCodeViewModel>();
                List<InviteCodeViewModel> claimedCodes = new List<InviteCodeViewModel>();
                if (user.OwnedInviteCodes != null)
                {
                    foreach (InviteCode inviteCode in user.OwnedInviteCodes.Where(c => c.Active))
                    {
                        InviteCodeViewModel inviteCodeViewModel = new InviteCodeViewModel();
                        inviteCodeViewModel.ClaimedUser = inviteCode.ClaimedUser;
                        inviteCodeViewModel.Active = inviteCode.Active;
                        inviteCodeViewModel.Code = inviteCode.Code;
                        inviteCodeViewModel.InviteCodeId = inviteCode.InviteCodeId;
                        inviteCodeViewModel.Owner = inviteCode.Owner;

                        if (inviteCode.ClaimedUser == null)
                            availableCodes.Add(inviteCodeViewModel);

                        if (inviteCode.ClaimedUser != null)
                            claimedCodes.Add(inviteCodeViewModel);
                    }
                }

                model.AvailableCodes = availableCodes;
                model.ClaimedCodes = claimedCodes;

                return View("/Areas/User/Views/User/Settings/InviteSettings.cshtml", model);
            }

            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }
        
        public IActionResult BlogSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                HttpContext.Session.Set(_AuthSessionKey, user.Username);

                ViewBag.Title = "Blog Settings - " + _config.Title;
                ViewBag.Description = "Your " + _config.Title + " Blog Settings";

                BlogSettingsViewModel model = new BlogSettingsViewModel();
                model.Page = "Blog";
                model.UserID = user.UserId;
                model.Username = user.Username;
                model.Title = user.BlogSettings.Title;
                model.Description = user.BlogSettings.Description;

                return View("/Areas/User/Views/User/Settings/BlogSettings.cshtml", model);
            }

            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }
        
        public IActionResult UploadSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                HttpContext.Session.Set(_AuthSessionKey, user.Username);

                ViewBag.Title = "Upload Settings - " + _config.Title;
                ViewBag.Description = "Your " + _config.Title + " Upload Settings";

                UploadSettingsViewModel model = new UploadSettingsViewModel();
                model.Page = "Upload";
                model.UserID = user.UserId;
                model.Username = user.Username;
                model.Encrypt = user.UploadSettings.Encrypt;

                return View("/Areas/User/Views/User/Settings/UploadSettings.cshtml", model);
            }

            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewRawPGP(string username)
        {
            ViewBag.Title = username + "'s Public Key - " + _config.Title;
            ViewBag.Description = "The PGP public key for " + username;

            User user = UserHelper.GetUser(_dbContext, username);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.SecuritySettings.PGPSignature))
                {
                    return Content(user.SecuritySettings.PGPSignature, "text/plain");
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBlog(BlogSettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        // Blogs
                        user.BlogSettings.Title = settings.Title;
                        user.BlogSettings.Description = settings.Description;

                        UserHelper.EditAccount(_dbContext, _config, user);
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
        public IActionResult EditProfile(ProfileSettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        // Profile Info
                        user.UserSettings.Website = settings.Website;
                        user.UserSettings.Quote = settings.Quote;
                        user.UserSettings.About = settings.About;

                        UserHelper.EditAccount(_dbContext, _config, user);
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
        public IActionResult EditSecurity(SecuritySettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        bool changePass = false;
                        // Changing Password?
                        if (!string.IsNullOrEmpty(settings.CurrentPassword) && (!string.IsNullOrEmpty(settings.NewPassword) || !string.IsNullOrEmpty(settings.NewPasswordConfirm)))
                        {
                            // Old Password Valid?
                            if (!UserHelper.UserPasswordCorrect(_dbContext, _config, user, settings.CurrentPassword))
                            {
                                return Json(new { error = "Invalid Original Password." });
                            }
                            // The New Password Match?
                            if (settings.NewPassword != settings.NewPasswordConfirm)
                            {
                                return Json(new { error = "New Password Must Match." });
                            }
                            // Are password resets enabled?
                            if (!_config.UserConfig.PasswordResetEnabled)
                            {
                                return Json(new { error = "Password resets are disabled." });
                            }
                            changePass = true;
                        }

                        // PGP Key valid?
                        if (!string.IsNullOrEmpty(settings.PgpPublicKey) && !PGP.IsPublicKey(settings.PgpPublicKey))
                        {
                            return Json(new { error = "Invalid PGP Public Key" });
                        }
                        user.SecuritySettings.PGPSignature = settings.PgpPublicKey;

                        // Recovery Email
                        bool newRecovery = false;
                        if (settings.RecoveryEmail != user.SecuritySettings.RecoveryEmail)
                        {
                            newRecovery = true;
                            user.SecuritySettings.RecoveryEmail = settings.RecoveryEmail;
                            user.SecuritySettings.RecoveryVerified = false;
                        }

                        // Trusted Devices
                        user.SecuritySettings.AllowTrustedDevices = settings.AllowTrustedDevices;
                        if (!settings.AllowTrustedDevices)
                        {
                            // They turned it off, let's clear the trusted devices
                            user.TrustedDevices.Clear();
                            List<TrustedDevice> foundDevices = _dbContext.TrustedDevices.Where(d => d.UserId == user.UserId).ToList();
                            if (foundDevices != null)
                            {
                                foreach (TrustedDevice device in foundDevices)
                                {
                                    _dbContext.TrustedDevices.Remove(device);
                                }
                            }
                        }

                        // Two Factor Authentication
                        bool oldTwoFactor = user.SecuritySettings.TwoFactorEnabled;
                        user.SecuritySettings.TwoFactorEnabled = settings.TwoFactorEnabled;
                        string newKey = string.Empty;
                        if (!oldTwoFactor && settings.TwoFactorEnabled)
                        {
                            // They just enabled it, let's regen the key
                            newKey = Authenticator.GenerateKey();

                            // New key, so let's upsert their key into git
                            if (_config.GitConfig.Enabled)
                            {
                                UserHelper.CreateUserGitTwoFactor(_config, user.Username, newKey, DateTimeHelper.GetUnixTimestamp());
                            }
                        }
                        else if (!settings.TwoFactorEnabled)
                        {
                            // remove the key when it's disabled
                            newKey = string.Empty;

                            // Removed the key, so delete it from git as well
                            if (_config.GitConfig.Enabled)
                            {
                                UserHelper.DeleteUserGitTwoFactor(_config, user.Username);
                            }
                        }
                        else
                        {
                            // No change, let's use the old value
                            newKey = user.SecuritySettings.TwoFactorKey;
                        }
                        user.SecuritySettings.TwoFactorKey = newKey;

                        UserHelper.EditAccount(_dbContext, _config, user, changePass, settings.NewPassword);

                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(settings.RecoveryEmail) && newRecovery)
                        {
                            string verifyCode = UserHelper.CreateRecoveryEmailVerification(_dbContext, _config, user);
                            string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = user.Username });
                            string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                            UserHelper.SendRecoveryEmailVerification(_config, user.Username, user.SecuritySettings.RecoveryEmail, resetUrl, verifyUrl);
                        }

                        if (!oldTwoFactor && settings.TwoFactorEnabled)
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
        public IActionResult EditUpload(UploadSettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        // Profile Info
                        user.UploadSettings.Encrypt = settings.Encrypt;

                        UserHelper.EditAccount(_dbContext, _config, user);
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
        public async Task<IActionResult> Delete()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        UserHelper.DeleteAccount(_dbContext, _config, user);

                        // Sign Out
                        await AccountsController.LogoutUser(User, HttpContext, _signInManager, _events);

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
        public IActionResult VerifyRecoveryEmail(string code)
        {
            bool verified = true;
            if (string.IsNullOrEmpty(code))
                verified &= false;

            // Is there a code?
            if (verified)
            {
                verified &= UserHelper.VerifyRecoveryEmail(_dbContext, _config, User.Identity.Name, code);
            }

            RecoveryEmailVerificationViewModel model = new RecoveryEmailVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ViewRecoveryEmailVerification.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendVerifyRecoveryEmail()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(user.SecuritySettings.RecoveryEmail))
                        {
                            if (!user.SecuritySettings.RecoveryVerified)
                            {
                                string verifyCode = UserHelper.CreateRecoveryEmailVerification(_dbContext, _config, user);
                                string resetUrl = Url.SubRouteUrl("user", "User.ResetPassword", new { Username = user.Username });
                                string verifyUrl = Url.SubRouteUrl("user", "User.VerifyRecoveryEmail", new { Code = verifyCode });
                                UserHelper.SendRecoveryEmailVerification(_config, user.Username, user.SecuritySettings.RecoveryEmail, resetUrl, verifyUrl);
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
        public IActionResult ResetPassword(string username)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Username = username;

            return View("/Areas/User/Views/User/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SendResetPasswordVerification(string username)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, username);
                    if (user != null)
                    {
                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(user.SecuritySettings.RecoveryEmail) && user.SecuritySettings.RecoveryVerified)
                        {
                            string verifyCode = UserHelper.CreateResetPasswordVerification(_dbContext, _config, user);
                            string resetUrl = Url.SubRouteUrl("user", "User.VerifyResetPassword", new { Username = user.Username, Code = verifyCode });
                            UserHelper.SendResetPasswordVerification(_config, user.Username, user.SecuritySettings.RecoveryEmail, resetUrl);
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
        public IActionResult VerifyResetPassword(string username, string code)
        {
            bool verified = true;
            if (string.IsNullOrEmpty(code))
                verified &= false;

            // Is there a code?
            if (verified)
            {
                verified &= UserHelper.VerifyResetPassword(_dbContext, _config, username, code);

                if (verified)
                {
                    // The password reset code is valid, let's get their user account for this session
                    User user = UserHelper.GetUser(_dbContext, username);
                    HttpContext.Session.Set(_AuthSessionKey, user.Username);
                    HttpContext.Session.Set("AuthCode", code);
                }
            }

            ResetPasswordVerificationViewModel model = new ResetPasswordVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ResetPasswordVerification.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SetUserPassword(SetPasswordViewModel passwordViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string code = HttpContext.Session.Get<string>("AuthCode");
                    if (!string.IsNullOrEmpty(code))
                    {
                        string username = HttpContext.Session.Get<string>(_AuthSessionKey);
                        if (!string.IsNullOrEmpty(username))
                        {
                            if (string.IsNullOrEmpty(passwordViewModel.Password))
                            {
                                return Json(new { error = "Password must not be empty" });
                            }
                            if (passwordViewModel.Password != passwordViewModel.PasswordConfirm)
                            {
                                return Json(new { error = "Passwords must match" });
                            }

                            User newUser = UserHelper.GetUser(_dbContext, username);
                            UserHelper.EditAccount(_dbContext, _config, newUser, true, passwordViewModel.Password);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyAuthenticatorCode(string code)
        {
            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
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
        public IActionResult GenerateAuthQrCode(string key)
        {
            var ProvisionUrl = string.Format("otpauth://totp/{0}:{1}?secret={2}", _config.Title, User.Identity.Name, key);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(ProvisionUrl, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            return File(qrCode.GetGraphic(20), "image/png");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearTrustedDevices()
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    if (user.SecuritySettings.AllowTrustedDevices)
                    {
                        // let's clear the trusted devices
                        user.TrustedDevices.Clear();
                        List<TrustedDevice> foundDevices = _dbContext.TrustedDevices.Where(d => d.UserId == user.UserId).ToList();
                        if (foundDevices != null)
                        {
                            foreach (TrustedDevice device in foundDevices)
                            {
                                _dbContext.TrustedDevices.Remove(device);
                            }
                        }
                        _dbContext.Entry(user).State = EntityState.Modified;
                        _dbContext.SaveChanges();

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
        public async Task<IActionResult> GenerateToken(string name, [FromServices] ICompositeViewEngine viewEngine)
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    string newTokenStr = UserHelper.GenerateAuthToken(_dbContext, user.Username);

                    if (!string.IsNullOrEmpty(newTokenStr))
                    {
                        AuthToken token = new AuthToken();
                        token.UserId = user.UserId;
                        token.HashedToken = SHA256.Hash(newTokenStr);
                        token.Name = name;

                        _dbContext.AuthTokens.Add(token);
                        _dbContext.SaveChanges();

                        AuthTokenViewModel model = new AuthTokenViewModel();
                        model.AuthTokenId = token.AuthTokenId;
                        model.Name = token.Name;
                        model.LastDateUsed = token.LastDateUsed;

                        string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/User/Views/User/Settings/AuthToken.cshtml", model);

                        return Json(new { result = new { token = newTokenStr, html = renderedView } });
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
        public IActionResult RevokeAllTokens()
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    user.AuthTokens.Clear();
                    List<AuthToken> foundTokens = _dbContext.AuthTokens.Where(d => d.UserId == user.UserId).ToList();
                    if (foundTokens != null)
                    {
                        foreach (AuthToken token in foundTokens)
                        {
                            _dbContext.AuthTokens.Remove(token);
                        }
                    }
                    _dbContext.Entry(user).State = EntityState.Modified;
                    _dbContext.SaveChanges();

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
        public IActionResult EditTokenName(int tokenId, string name)
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    AuthToken foundToken = _dbContext.AuthTokens.Where(d => d.UserId == user.UserId && d.AuthTokenId == tokenId).FirstOrDefault();
                    if (foundToken != null)
                    {
                        foundToken.Name = name;
                        _dbContext.Entry(foundToken).State = EntityState.Modified;
                        _dbContext.SaveChanges();

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
        public IActionResult DeleteToken(int tokenId)
        {
            try
            {
                User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                if (user != null)
                {
                    AuthToken foundToken = _dbContext.AuthTokens.Where(d => d.UserId == user.UserId && d.AuthTokenId == tokenId).FirstOrDefault();
                    if (foundToken != null)
                    {
                        _dbContext.AuthTokens.Remove(foundToken);
                        user.AuthTokens.Remove(foundToken);
                        _dbContext.Entry(user).State = EntityState.Modified;
                        _dbContext.SaveChanges();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateInviteCodeLink(int inviteCodeId)
        {
            try
            {
                InviteCode code = _dbContext.InviteCodes.Where(c => c.InviteCodeId == inviteCodeId).FirstOrDefault();
                if (code != null)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        if (code.Owner.Username == User.Identity.Name)
                        {
                            return Json(new { result = Url.SubRouteUrl("user", "User.Register", new { inviteCode = code.Code }) });
                        }
                    }
                    return Json(new { error = "Invite Code not associated with this user" });
                }
                return Json(new { error = "Invalid Invite Code" });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        private async Task SignInUser(User user, string returnUrl, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Add their roles
            foreach (var role in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTime.UtcNow.AddMonths(1),
                RedirectUri = returnUrl
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProps);
        }
    }
}
