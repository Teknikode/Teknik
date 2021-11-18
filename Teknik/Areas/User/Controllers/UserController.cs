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
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using IdentityModel.Client;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Teknik.Security;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Models;
using Teknik.Utilities.Routing;
using Teknik.BillingCore;

namespace Teknik.Areas.Users.Controllers
{
    [Authorize]
    [Area("User")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class UserController : DefaultController
    {
        private static readonly UsedCodesManager usedCodesManager = new UsedCodesManager();
        private const string _AuthSessionKey = "AuthenticatedUser";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public UserController(ILogger<Logger> logger, Config config, TeknikEntities dbContext, IHttpContextAccessor httpContextAccessor) : base(logger, config, dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            // Let's double check their email and git accounts to make sure they exist
            string email = UserHelper.GetUserEmailAddress(_config, User.Identity.Name);
            if (_config.EmailConfig.Enabled && !UserHelper.UserEmailExists(_config, email))
            {
                //UserHelper.AddUserEmail(_config, email, model.Password);
            }

            if (_config.GitConfig.Enabled && !UserHelper.UserGitExists(_config, User.Identity.Name))
            {
                //UserHelper.AddUserGit(_config, User.Identity.Name, model.Password);
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }

        [HttpGet]
        [TrackPageView]
        public async Task Logout()
        {
            // these are the sub & sid to signout
            //var sub = User.FindFirst("sub")?.Value;
            //var sid = User.FindFirst("sid")?.Value;

            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            //_logoutSessions.Add(sub, sid);
        }

        [HttpGet]
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Register(string inviteCode, string ReturnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();
            model.InviteCode = inviteCode;
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/User/Views/User/ViewRegistration.cshtml", model);
        }

        [HttpOptions]
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
                    if (!model.Error && !(await UserHelper.UsernameAvailable(_dbContext, _config, model.Username)))
                    {
                        model.Error = true;
                        model.ErrorMessage = "That username is not available";
                    }
                    if (!model.Error && string.IsNullOrEmpty(model.Password))
                    {
                        model.Error = true;
                        model.ErrorMessage = "You must enter a password";
                    }
                    if (!model.Error && model.Password.Length < _config.UserConfig.MinPasswordLength)
                    {
                        model.Error = true;
                        model.ErrorMessage = $"Password must be at least {_config.UserConfig.MinPasswordLength} characters long";
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

                    if (!model.Error)
                    {
                        try
                        {
                            await UserHelper.CreateAccount(_dbContext, _config, Url, model.Username, model.Password, model.RecoveryEmail, model.InviteCode);
                        }
                        catch (Exception ex)
                        {
                            model.Error = true;
                            model.ErrorMessage = ex.GetFullMessage(true);
                        }
                        if (!model.Error)
                        {
                            // Let's log them in

                            return GenerateActionResult(new { success = true, redirectUrl = Url.SubRouteUrl("account", "User.Login", new { returnUrl = model.ReturnUrl }) }, Redirect(Url.SubRouteUrl("account", "User.Login", new { returnUrl = model.ReturnUrl })));
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
        [TrackPageView]
        public async Task<IActionResult> ViewProfile(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }

            ProfileViewModel model = new ProfileViewModel();
            ViewBag.Title = "User Does Not Exist";
            ViewBag.Description = "The User does not exist";

            try
            {
                User user = UserHelper.GetUser(_dbContext, username);

                if (user != null)
                {
                    ViewBag.Title = username + "'s Profile";
                    ViewBag.Description = "Viewing " + username + "'s Profile";

                    model.UserID = user.UserId;
                    model.Username = user.Username;
                    if (_config.EmailConfig.Enabled)
                    {
                        model.Email = string.Format("{0}@{1}", user.Username, _config.EmailConfig.Domain);
                    }

                    // Get the user claims for this user
                    model.IdentityUserInfo = await IdentityHelper.GetIdentityUserInfo(_config, user.Username);

                    model.LastSeen = UserHelper.GetLastAccountActivity(_dbContext, _config, user.Username, model.IdentityUserInfo);

                    model.UserSettings = user.UserSettings;
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

        [TrackPageView]
        public IActionResult ViewServiceData()
        {
            string username = User.Identity.Name;

            ViewServiceDataViewModel model = new ViewServiceDataViewModel();
            ViewBag.Title = "User Does Not Exist";
            ViewBag.Description = "The User does not exist";

            try
            {
                User user = UserHelper.GetUser(_dbContext, username);

                if (user != null)
                {
                    ViewBag.Title = "Service Data";
                    ViewBag.Description = "Viewing all of your service data";
                    
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

        [TrackPageView]
        public IActionResult Settings()
        {
            return Redirect(Url.SubRouteUrl("account", "User.ProfileSettings"));
        }

        [TrackPageView]
        public IActionResult ProfileSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Profile Settings";
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

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public IActionResult AccountSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Account Settings";
                ViewBag.Description = "Your " + _config.Title + " Account Settings";

                AccountSettingsViewModel model = new AccountSettingsViewModel();
                model.Page = "Account";
                model.UserID = user.UserId;
                model.Username = user.Username;

                return View("/Areas/User/Views/User/Settings/AccountSettings.cshtml", model);
            }

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public IActionResult BillingSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Billing Settings";
                ViewBag.Description = "Your " + _config.Title + " Billing Settings";

                BillingSettingsViewModel model = new BillingSettingsViewModel();
                model.Page = "Billing";
                model.UserID = user.UserId;
                model.Username = user.Username;

                if (user.BillingCustomer != null)
                {
                    var billingService = BillingFactory.GetBillingService(_config.BillingConfig);
                    var portalSession = billingService.CreatePortalSession(user.BillingCustomer.CustomerId, Url.SubRouteUrl("account", "User.BillingSettings"));
                    model.PortalUrl = portalSession.Url;

                    var subs = billingService.GetSubscriptionList(user.BillingCustomer.CustomerId);
                    foreach (var sub in subs)
                    {
                        foreach (var price in sub.Prices)
                        {
                            var product = billingService.GetProduct(price.ProductId);
                            var subView = new SubscriptionViewModel()
                            {
                                SubscriptionId = sub.Id,
                                ProductId = product.ProductId,
                                ProductName = product.Name,
                                Storage = price.Storage,
                                Price = price.Amount,
                                Interval = price.Interval.ToString(),
                            };
                            model.Subscriptions.Add(subView);
                        }
                    }
                }

                return View("/Areas/User/Views/User/Settings/BillingSettings.cshtml", model);
            }

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public async Task<IActionResult> SecuritySettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Security Settings";
                ViewBag.Description = "Your " + _config.Title + " Security Settings";

                SecuritySettingsViewModel model = new SecuritySettingsViewModel();
                model.Page = "Security";
                model.UserID = user.UserId;
                model.Username = user.Username;

                // Get the user secure info
                IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, user.Username);
                //model.TrustedDeviceCount = user.TrustedDevices.Count;
                //model.AuthTokens = new List<AuthTokenViewModel>();
                //foreach (AuthToken token in user.AuthTokens)
                //{
                //    AuthTokenViewModel tokenModel = new AuthTokenViewModel();
                //    tokenModel.AuthTokenId = token.AuthTokenId;
                //    tokenModel.Name = token.Name;
                //    tokenModel.LastDateUsed = token.LastDateUsed;

                //    model.AuthTokens.Add(tokenModel);
                //}

                model.PgpPublicKey = userInfo.PGPPublicKey;
                model.RecoveryEmail = userInfo.RecoveryEmail;
                if (userInfo.RecoveryVerified.HasValue)
                    model.RecoveryVerified = userInfo.RecoveryVerified.Value;
                if (userInfo.TwoFactorEnabled.HasValue)
                    model.TwoFactorEnabled = userInfo.TwoFactorEnabled.Value;
                
                return View("/Areas/User/Views/User/Settings/SecuritySettings.cshtml", model);
            }

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public async Task<IActionResult> DeveloperSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Developer Settings";
                ViewBag.Description = "Your " + _config.Title + " Developer Settings";

                DeveloperSettingsViewModel model = new DeveloperSettingsViewModel();
                model.Page = "Developer";
                model.UserID = user.UserId;
                model.Username = user.Username;

                model.AuthTokens = new List<AuthTokenViewModel>();
                model.Clients = new List<ClientViewModel>();
                //foreach (AuthToken token in user.AuthTokens)
                //{
                //    AuthTokenViewModel tokenModel = new AuthTokenViewModel();
                //    tokenModel.AuthTokenId = token.AuthTokenId;
                //    tokenModel.Name = token.Name;
                //    tokenModel.LastDateUsed = token.LastDateUsed;

                //    model.AuthTokens.Add(tokenModel);
                //}

                Client[] clients = await IdentityHelper.GetClients(_config, username);
                foreach (Client client in clients)
                {
                    model.Clients.Add(new ClientViewModel()
                    {
                        Id = client.ClientId,
                        Name = client.ClientName,
                        HomepageUrl = client.ClientUri,
                        LogoUrl = client.LogoUri,
                        CallbackUrl = string.Join(',', client.RedirectUris),
                        AllowedScopes = client.AllowedScopes,
                        GrantType = IdentityHelper.GrantsToGrantType(client.AllowedGrantTypes.ToArray())
                    });
                }

                return View("/Areas/User/Views/User/Settings/DeveloperSettings.cshtml", model);
            }

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public IActionResult InviteSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Invite Settings";
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

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public IActionResult BlogSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Blog Settings";
                ViewBag.Description = "Your " + _config.Title + " Blog Settings";

                BlogSettingsViewModel model = new BlogSettingsViewModel();
                model.Page = "Blog";
                model.UserID = user.UserId;
                model.Username = user.Username;
                model.Title = user.BlogSettings.Title;
                model.Description = user.BlogSettings.Description;

                return View("/Areas/User/Views/User/Settings/BlogSettings.cshtml", model);
            }

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [TrackPageView]
        public IActionResult UploadSettings()
        {
            string username = User.Identity.Name;
            User user = UserHelper.GetUser(_dbContext, username);

            if (user != null)
            {
                ViewBag.Title = "Upload Settings";
                ViewBag.Description = "Your " + _config.Title + " Upload Settings";

                UploadSettingsViewModel model = new UploadSettingsViewModel();
                model.Page = "Upload";
                model.UserID = user.UserId;
                model.Username = user.Username;

                model.MaxStorage = user.UploadSettings.MaxUploadStorage ?? _config.UploadConfig.MaxStorage;
                model.MaxFileSize = user.UploadSettings.MaxUploadFileSize ?? _config.UploadConfig.MaxUploadFileSize;

                model.Encrypt = user.UploadSettings.Encrypt;
                model.ExpirationLength = user.UploadSettings.ExpirationLength;
                model.ExpirationUnit = user.UploadSettings.ExpirationUnit;

                return View("/Areas/User/Views/User/Settings/UploadSettings.cshtml", model);
            }

            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [HttpGet]
        [AllowAnonymous]
        [TrackPageView]
        public async Task<IActionResult> ViewRawPGP(string username)
        {
            ViewBag.Title = username + "'s Public Key";
            ViewBag.Description = "The PGP public key for " + username;
            
            IdentityUserInfo userClaims = await IdentityHelper.GetIdentityUserInfo(_config, username);
            if (!string.IsNullOrEmpty(userClaims.PGPPublicKey))
            {
                return Content(userClaims.PGPPublicKey, "text/plain");
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
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
        public async Task<IActionResult> EditSecurity(SecuritySettingsViewModel settings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        // PGP Key valid?
                        if (!string.IsNullOrEmpty(settings.PgpPublicKey) && !PGP.IsPublicKey(settings.PgpPublicKey))
                        {
                            return Json(new { error = "Invalid PGP Public Key" });
                        }

                        // Get the user secure info
                        IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, user.Username);

                        if (userInfo.PGPPublicKey != settings.PgpPublicKey)
                        {
                            var result = await IdentityHelper.UpdatePGPPublicKey(_config, user.Username, settings.PgpPublicKey);
                            if (!result.Success)
                                return Json(new { error = result.Message });
                        }

                        if (userInfo.RecoveryEmail != settings.RecoveryEmail)
                        {
                            var token = await IdentityHelper.UpdateRecoveryEmail(_config, user.Username, settings.RecoveryEmail);

                            // If they have a recovery email, let's send a verification
                            if (!string.IsNullOrEmpty(settings.RecoveryEmail))
                            {
                                string resetUrl = Url.SubRouteUrl("account", "User.ResetPassword", new { Username = user.Username });
                                string verifyUrl = Url.SubRouteUrl("account", "User.VerifyRecoveryEmail", new { Username = user.Username, Code = WebUtility.UrlEncode(token) });
                                UserHelper.SendRecoveryEmailVerification(_config, user.Username, settings.RecoveryEmail, resetUrl, verifyUrl);
                            }
                        }

                        //if (!settings.TwoFactorEnabled && (!userInfo.TwoFactorEnabled.HasValue || userInfo.TwoFactorEnabled.Value))
                        //{
                        //    var result = await IdentityHelper.Disable2FA(_config, user.Username);
                        //    if (!result.Success)
                        //        return Json(new { error = result.Message });
                        //}

                        //UserHelper.EditAccount(_dbContext, _config, user, changePass, settings.NewPassword);


                        //if (!oldTwoFactor && settings.TwoFactorEnabled)
                        //{
                        //    return Json(new { result = new { checkAuth = true, key = newKey, qrUrl = Url.SubRouteUrl("account", "User.Action", new { action = "GenerateAuthQrCode", key = newKey }) } });
                        //}
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
                        user.UploadSettings.ExpirationUnit = settings.ExpirationUnit;
                        user.UploadSettings.ExpirationLength = settings.ExpirationLength;

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

        public async Task<IActionResult> ChangePassword(AccountSettingsViewModel settings)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        // Did they enter their old password
                        if (string.IsNullOrEmpty(settings.CurrentPassword))
                            return Json(new { error = "You must enter your current password" });
                        // Did they enter a new password
                        if (string.IsNullOrEmpty(settings.NewPassword) || string.IsNullOrEmpty(settings.NewPasswordConfirm))
                            return Json(new { error = "You must enter your new password" });
                        // Old Password Valid?
                        if (!(await UserHelper.UserPasswordCorrect(_config, user.Username, settings.CurrentPassword)))
                            return Json(new { error = "Invalid Original Password" });
                        // Does the new password meet the length requirement?
                        if (settings.NewPassword.Length < _config.UserConfig.MinPasswordLength)
                            return Json(new { error = $"New Password must be at least {_config.UserConfig.MinPasswordLength} characters long" });
                        // The New Password Match?
                        if (settings.NewPassword != settings.NewPasswordConfirm)
                            return Json(new { error = "New Password must match confirmation" });
                        // Are password resets enabled?
                        if (!_config.UserConfig.PasswordResetEnabled)
                            return Json(new { error = "Password resets are disabled" });

                        // Change their password
                        await UserHelper.ChangeAccountPassword(_dbContext, _config, user.Username, settings.CurrentPassword, settings.NewPassword);

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
                        await UserHelper.DeleteAccount(_dbContext, _config, user);

                        // Sign Out
                        await HttpContext.SignOutAsync("Cookies");
                        await HttpContext.SignOutAsync("oidc");

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
        public async Task<IActionResult> VerifyRecoveryEmail(string username, string code)
        {
            bool verified = true;
            if (string.IsNullOrEmpty(code))
                verified &= false;

            // Is there a code?
            if (verified)
            {
                var result = await IdentityHelper.VerifyRecoveryEmail(_config, username, WebUtility.UrlDecode(code));
                verified &= result.Success;
            }

            RecoveryEmailVerificationViewModel model = new RecoveryEmailVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ViewRecoveryEmailVerification.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerifyRecoveryEmail()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    IdentityUserInfo userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                    User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
                    if (user != null)
                    {
                        //If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(userInfo.RecoveryEmail))
                        {
                            if (!userInfo.RecoveryVerified.HasValue || !userInfo.RecoveryVerified.Value)
                            {
                                var token = await IdentityHelper.UpdateRecoveryEmail(_config, user.Username, userInfo.RecoveryEmail);
                                string resetUrl = Url.SubRouteUrl("account", "User.ResetPassword", new { Username = user.Username });
                                string verifyUrl = Url.SubRouteUrl("account", "User.VerifyRecoveryEmail", new { Username = user.Username, Code = WebUtility.UrlEncode(token) });
                                UserHelper.SendRecoveryEmailVerification(_config, user.Username, userInfo.RecoveryEmail, resetUrl, verifyUrl);
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
        [TrackPageView]
        public IActionResult ResetPassword(string username)
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Username = username;

            return View("/Areas/User/Views/User/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetPasswordVerification(string username)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = UserHelper.GetUser(_dbContext, username);
                    if (user != null)
                    {
                        IdentityUserInfo userClaims = await IdentityHelper.GetIdentityUserInfo(_config, user.Username);
                        // If they have a recovery email, let's send a verification
                        if (!string.IsNullOrEmpty(userClaims.RecoveryEmail) && userClaims.RecoveryVerified.HasValue && userClaims.RecoveryVerified.Value)
                        {
                            string verifyCode = await IdentityHelper.GeneratePasswordResetToken(_config, user.Username);
                            string resetUrl = Url.SubRouteUrl("account", "User.VerifyResetPassword", new { Username = user.Username, Code = WebUtility.UrlEncode(verifyCode) });
                            UserHelper.SendResetPasswordVerification(_config, user.Username, userClaims.RecoveryEmail, resetUrl);
                            return Json(new { result = true });
                        }
                        return Json(new { error = "The user doesn't have a recovery email specified, or has not been verified." });
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
        [TrackPageView]
        public async Task<IActionResult> VerifyResetPassword(string username, string code)
        {
            bool verified = true;
            if (string.IsNullOrEmpty(code))
                verified &= false;

            // Is there a code?
            if (verified)
            {
                // The password reset code is valid, let's get their user account for this session
                User user = UserHelper.GetUser(_dbContext, username);
                _session.SetString(_AuthSessionKey, user.Username);
                _session.SetString("AuthCode", WebUtility.UrlDecode(code));

                await _session.CommitAsync();
            }

            ResetPasswordVerificationViewModel model = new ResetPasswordVerificationViewModel();
            model.Success = verified;

            return View("/Areas/User/Views/User/ResetPasswordVerification.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetUserPassword(SetPasswordViewModel passwordViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _session.LoadAsync();
                    string code = _session.GetString("AuthCode");
                    if (!string.IsNullOrEmpty(code))
                    {
                        string username = _session.GetString(_AuthSessionKey);
                        if (!string.IsNullOrEmpty(username))
                        {
                            if (string.IsNullOrEmpty(passwordViewModel.Password))
                            {
                                return Json(new { error = "Password must not be empty" });
                            }
                            if (passwordViewModel.Password.Length < _config.UserConfig.MinPasswordLength)
                            {
                                return Json(new { error = $"Password must be at least {_config.UserConfig.MinPasswordLength} characters long" });
                            }
                            if (passwordViewModel.Password != passwordViewModel.PasswordConfirm)
                            {
                                return Json(new { error = "Passwords must match" });
                            }

                            try
                            {
                                await UserHelper.ResetAccountPassword(_dbContext, _config, username, code, passwordViewModel.Password);

                                _session.Remove(_AuthSessionKey);
                                _session.Remove("AuthCode");

                                return Json(new { result = true });
                            }
                            catch (Exception ex)
                            {
                                return Json(new { error = ex.Message });
                            }
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
        public async Task<IActionResult> Generate2FA()
        {
            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
            if (user != null)
            {
                // Get User Identity Info
                var userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                if (userInfo.TwoFactorEnabled.HasValue && !userInfo.TwoFactorEnabled.Value)
                {
                    // Validate the code with the identity server
                    var key = await IdentityHelper.Reset2FAKey(_config, user.Username);

                    if (!string.IsNullOrEmpty(key))
                    {
                        return Json(new { result = true, key = key, qrUrl = Url.SubRouteUrl("account", "User.Action", new { action = "GenerateAuthQrCode", key = key }) });
                    }
                    return Json(new { error = "Unable to generate Two Factor Authentication key" });
                }
                return Json(new { error = "User already has Two Factor Authentication enabled" });
            }
            return Json(new { error = "User does not exist" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(string code)
        {
            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
            if (user != null)
            {
                // Get User Identity Info
                var userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                if (userInfo.TwoFactorEnabled.HasValue && !userInfo.TwoFactorEnabled.Value)
                {
                    // Validate the code with the identity server
                    var result = await IdentityHelper.Enable2FA(_config, user.Username, code);

                    if (result.Any())
                    {
                        return Json(new { result = true, recoveryCodes = result });
                    }
                    return Json(new { error = "Invalid Authentication Code" });
                }
                return Json(new { error = "User already has Two Factor Authentication enabled" });
            }
            return Json(new { error = "User does not exist" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetRecoveryCodes()
        {
            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
            if (user != null)
            {
                // Get User Identity Info
                var userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                if (userInfo.TwoFactorEnabled.HasValue && userInfo.TwoFactorEnabled.Value)
                {
                    // Regenerate the recovery codes
                    var result = await IdentityHelper.GenerateRecoveryCodes(_config, user.Username);

                    if (result.Any())
                    {
                        return Json(new { result = true, recoveryCodes = result });
                    }
                    return Json(new { error = "Invalid Authentication Code" });
                }
                return Json(new { error = "User doesn't have Two Factor Authentication enabled" });
            }
            return Json(new { error = "User does not exist" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2FA()
        {
            User user = UserHelper.GetUser(_dbContext, User.Identity.Name);
            if (user != null)
            {
                // Get User Identity Info
                var userInfo = await IdentityHelper.GetIdentityUserInfo(_config, User.Identity.Name);
                if (userInfo.TwoFactorEnabled.HasValue && userInfo.TwoFactorEnabled.Value)
                {
                    // Validate the code with the identity server
                    var result = await IdentityHelper.Disable2FA(_config, user.Username);

                    if (result.Success)
                    {
                        return Json(new { result = true });
                    }
                    return Json(new { error = result.Message });
                }
                return Json(new { error = "User doesn't have Two Factor Authentication enabled" });
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
                    //if (user.SecuritySettings.AllowTrustedDevices)
                    //{
                    //    // let's clear the trusted devices
                    //    user.TrustedDevices.Clear();
                    //    List<TrustedDevice> foundDevices = _dbContext.TrustedDevices.Where(d => d.UserId == user.UserId).ToList();
                    //    if (foundDevices != null)
                    //    {
                    //        foreach (TrustedDevice device in foundDevices)
                    //        {
                    //            _dbContext.TrustedDevices.Remove(device);
                    //        }
                    //    }
                    //    _dbContext.Entry(user).State = EntityState.Modified;
                    //    _dbContext.SaveChanges();

                    //    return Json(new { result = true });
                    //}
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
                    //string newTokenStr = UserHelper.GenerateAuthToken(_dbContext, user.Username);

                    //if (!string.IsNullOrEmpty(newTokenStr))
                    //{
                    //    AuthToken token = new AuthToken();
                    //    token.UserId = user.UserId;
                    //    token.HashedToken = SHA256.Hash(newTokenStr);
                    //    token.Name = name;

                    //    _dbContext.AuthTokens.Add(token);
                    //    _dbContext.SaveChanges();

                    //    AuthTokenViewModel model = new AuthTokenViewModel();
                    //    model.AuthTokenId = token.AuthTokenId;
                    //    model.Name = token.Name;
                    //    model.LastDateUsed = token.LastDateUsed;

                    //    string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/User/Views/User/Settings/AuthToken.cshtml", model);

                    //    return Json(new { result = new { token = newTokenStr, html = renderedView } });
                    //}
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
                    //user.AuthTokens.Clear();
                    //List<AuthToken> foundTokens = _dbContext.AuthTokens.Where(d => d.UserId == user.UserId).ToList();
                    //if (foundTokens != null)
                    //{
                    //    foreach (AuthToken token in foundTokens)
                    //    {
                    //        _dbContext.AuthTokens.Remove(token);
                    //    }
                    //}
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
                    //AuthToken foundToken = _dbContext.AuthTokens.Where(d => d.UserId == user.UserId && d.AuthTokenId == tokenId).FirstOrDefault();
                    //if (foundToken != null)
                    //{
                    //    foundToken.Name = name;
                    //    _dbContext.Entry(foundToken).State = EntityState.Modified;
                    //    _dbContext.SaveChanges();

                    //    return Json(new { result = new { name = name } });
                    //}
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
                    //AuthToken foundToken = _dbContext.AuthTokens.Where(d => d.UserId == user.UserId && d.AuthTokenId == tokenId).FirstOrDefault();
                    //if (foundToken != null)
                    //{
                    //    _dbContext.AuthTokens.Remove(foundToken);
                    //    user.AuthTokens.Remove(foundToken);
                    //    _dbContext.Entry(user).State = EntityState.Modified;
                    //    _dbContext.SaveChanges();

                    //    return Json(new { result = true });
                    //}
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
        public async Task<IActionResult> CreateClient(string name, string homepageUrl, string logoUrl, string callbackUrl, IdentityClientGrant grantType, string scopes, [FromServices] ICompositeViewEngine viewEngine)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return Json(new { error = "You must enter a client name" });
                if (string.IsNullOrEmpty(callbackUrl))
                    return Json(new { error = "You must enter an authorization callback URL" });
                if (!callbackUrl.IsValidUrl())
                    return Json(new { error = "Invalid callback URL" });
                if (!string.IsNullOrEmpty(homepageUrl) && !homepageUrl.IsValidUrl())
                    return Json(new { error = "Invalid homepage URL" });
                if (!string.IsNullOrEmpty(logoUrl) && !logoUrl.IsValidUrl())
                    return Json(new { error = "Invalid logo URL" });

                // Validate the code with the identity server
                var result = await IdentityHelper.CreateClient(
                    _config, 
                    User.Identity.Name, 
                    name, 
                    homepageUrl, 
                    logoUrl, 
                    callbackUrl,
                    IdentityHelper.GrantTypeToGrants(grantType),
                    scopes.Split(','));

                if (result.Success)
                {
                    var client = (JObject)result.Data;

                    ClientViewModel model = new ClientViewModel();
                    model.Id = client["id"].ToString();
                    model.Name = name;
                    model.HomepageUrl = homepageUrl;
                    model.LogoUrl = logoUrl;
                    model.CallbackUrl = callbackUrl;
                    model.GrantType = grantType;
                    model.AllowedScopes = scopes.Split(',');

                    string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/User/Views/User/Settings/ClientView.cshtml", model);

                    return Json(new { result = true, clientId = client["id"].ToString(), secret = client["secret"].ToString(), html = renderedView });
                }
                return Json(new { error = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetClient(string clientId)
        {
            Client foundClient = await IdentityHelper.GetClient(_config, User.Identity.Name, clientId);
            if (foundClient != null)
            {
                ClientModifyViewModel model = new ClientModifyViewModel()
                {
                    Id = foundClient.ClientId,
                    Name = foundClient.ClientName,
                    HomepageUrl = foundClient.ClientUri,
                    LogoUrl = foundClient.LogoUri,
                    CallbackUrl = string.Join(',', foundClient.RedirectUris),
                    AllowedScopes = foundClient.AllowedScopes,
                    GrantType = IdentityHelper.GrantsToGrantType(foundClient.AllowedGrantTypes.ToArray()).ToString()
                };

                return Json(new { result = true, client = model });
            }
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClient(string clientId, string name, string homepageUrl, string logoUrl, string callbackUrl, IdentityClientGrant grantType, string scopes, [FromServices] ICompositeViewEngine viewEngine)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return Json(new { error = "You must enter a client name" });
                if (string.IsNullOrEmpty(callbackUrl))
                    return Json(new { error = "You must enter an authorization callback URL" });
                if (!callbackUrl.IsValidUrl())
                    return Json(new { error = "Invalid callback URL" });
                if (!string.IsNullOrEmpty(homepageUrl) && !homepageUrl.IsValidUrl())
                    return Json(new { error = "Invalid homepage URL" });
                if (!string.IsNullOrEmpty(logoUrl) && !logoUrl.IsValidUrl())
                    return Json(new { error = "Invalid logo URL" });

                Client foundClient = await IdentityHelper.GetClient(_config, User.Identity.Name, clientId);

                if (foundClient == null)
                    return Json(new { error = "Client does not exist" });

                // Validate the code with the identity server
                var result = await IdentityHelper.EditClient(
                    _config, 
                    User.Identity.Name, 
                    clientId, 
                    name, 
                    homepageUrl, 
                    logoUrl, 
                    callbackUrl,
                    IdentityHelper.GrantTypeToGrants(grantType),
                    scopes.Split(','));

                if (result.Success)
                {
                    var client = (JObject)result.Data;

                    ClientViewModel model = new ClientViewModel();
                    model.Id = clientId;
                    model.Name = name;
                    model.HomepageUrl = homepageUrl;
                    model.LogoUrl = logoUrl;
                    model.CallbackUrl = callbackUrl;
                    model.GrantType = grantType;
                    model.AllowedScopes = scopes.Split(',');

                    string renderedView = await RenderPartialViewToString(viewEngine, "~/Areas/User/Views/User/Settings/ClientView.cshtml", model);

                    return Json(new { result = true, clientId = clientId, html = renderedView });
                }
                return Json(new { error = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.GetFullMessage(true) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClient(string clientId)
        {
            try
            {
                // Validate the code with the identity server
                var result = await IdentityHelper.DeleteClient(_config, clientId);

                if (result.Success)
                {
                    return Json(new { result = true });
                }
                return Json(new { error = result.Message });
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
                            return Json(new { result = Url.SubRouteUrl("account", "User.Register", new { inviteCode = code.Code }) });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteData(string type, string id)
        {
            var context = new ControllerContext();
            context.HttpContext = Request.HttpContext;
            context.RouteData = RouteData;
            context.ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor();

            switch (type)
            {
                case "upload":
                    var uploadController = new Upload.Controllers.UploadController(_logger, _config, _dbContext);
                    uploadController.ControllerContext = context;
                    return uploadController.Delete(id);
                case "paste":
                    var pasteController = new Paste.Controllers.PasteController(_logger, _config, _dbContext);
                    pasteController.ControllerContext = context;
                    return pasteController.Delete(id);
                case "shortenedUrl":
                    var shortenController = new Shortener.Controllers.ShortenerController(_logger, _config, _dbContext);
                    shortenController.ControllerContext = context;
                    return shortenController.Delete(id);
                case "vault":
                    var vaultController = new Vault.Controllers.VaultController(_logger, _config, _dbContext);
                    vaultController.ControllerContext = context;
                    return vaultController.Delete(id);
            }
            return Json(new { error = "Invalid Type" });
        }
    }
}
