using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Teknik.Areas.Accounts.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Logging;
using Teknik.Security;
using Teknik.Utilities;
using TwoStepsAuthenticator;

namespace Teknik.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountsController : DefaultController
    {
        private readonly UserStore _users;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;

        private static readonly UsedCodesManager _usedCodesManager = new UsedCodesManager();

        private const string _AuthSessionKey = "AuthenticatedUser";

        public AccountsController(
            ILogger<Logger> logger, 
            Config config, 
            TeknikEntities dbContext,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            UserStore users = null) 
            : base(logger, config, dbContext)
        {
            _users = users ?? new UserStore(_dbContext, _config);

            _signInManager = signInManager;
            _userManager = userManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string ReturnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/Accounts/Views/Accounts/ViewLogin.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind(Prefix = "Login")]LoginViewModel model)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (ModelState.IsValid)
            {
                string username = model.Username;
                User user = UserHelper.GetUser(_dbContext, username);
                if (user != null)
                {
                    // Make sure they aren't banned or anything
                    if (user.AccountStatus == AccountStatus.Banned)
                    {
                        model.Error = true;
                        model.ErrorMessage = "Account has been banned.";

                        // Raise the error event
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, model.ErrorMessage));

                        return GenerateActionResult(new { error = model.ErrorMessage }, View("/Areas/Accounts/Views/Accounts/ViewLogin.cshtml", model));
                    }

                    // Try to sign them in
                    var valid = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (valid)
                    {
                        // Perform transfer actions on the account
                        UserHelper.TransferUser(_dbContext, _config, user, model.Password);
                        user.LastSeen = DateTime.Now;
                        _dbContext.Entry(user).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        // Let's double check their email and git accounts to make sure they exist
                        string email = UserHelper.GetUserEmailAddress(_config, username);
                        if (_config.EmailConfig.Enabled && !UserHelper.UserEmailExists(_config, email))
                        {
                            UserHelper.AddUserEmail(_config, email, model.Password);
                        }

                        if (_config.GitConfig.Enabled && !UserHelper.UserGitExists(_config, username))
                        {
                            UserHelper.AddUserGit(_config, username, model.Password);
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
                                string token = Request.Cookies[Constants.TRUSTEDDEVICECOOKIE + "_" + username];
                                if (!string.IsNullOrEmpty(token))
                                {
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
                            HttpContext.Session.Set(_AuthSessionKey, user.Username);
                            if (string.IsNullOrEmpty(model.ReturnUrl))
                                returnUrl = Request.Headers["Referer"].ToString();
                            returnUrl = Url.SubRouteUrl("accounts", "Accounts.CheckAuthenticatorCode", new { returnUrl = returnUrl, rememberMe = model.RememberMe });
                            model.ReturnUrl = string.Empty;
                        }
                        else
                        {
                            returnUrl = Request.Headers["Referer"].ToString();

                            // They don't need two factor auth.
                            await SignInUser(user, (string.IsNullOrEmpty(model.ReturnUrl)) ? returnUrl : model.ReturnUrl, model.RememberMe);
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
            // Raise the error event
            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

            model.Error = true;
            model.ErrorMessage = "Invalid Username or Password.";

            return GenerateActionResult(new { error = model.ErrorMessage }, View("/Areas/Accounts/Views/Accounts/ViewLogin.cshtml", model));
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await LogoutUser(User, HttpContext, _signInManager, _events);

            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmTwoFactorAuth(string returnUrl, bool rememberMe)
        {
            string username = HttpContext.Session.Get<string>(_AuthSessionKey);
            if (!string.IsNullOrEmpty(username))
            {
                User user = UserHelper.GetUser(_dbContext, username);
                ViewBag.Title = "Unknown Device - " + _config.Title;
                ViewBag.Description = "We do not recognize this device.";
                TwoFactorViewModel model = new TwoFactorViewModel();
                model.ReturnUrl = returnUrl;
                model.RememberMe = rememberMe;
                model.AllowTrustedDevice = user.SecuritySettings.AllowTrustedDevices;

                return View("/Areas/Accounts/Views/Accounts/TwoFactorCheck.cshtml", model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmAuthenticatorCode(string code, string returnUrl, bool rememberMe, bool rememberDevice, string deviceName)
        {
            string errorMessage = string.Empty;
            string username = HttpContext.Session.Get<string>(_AuthSessionKey);
            if (!string.IsNullOrEmpty(username))
            {
                User user = UserHelper.GetUser(_dbContext, username);
                if (user.SecuritySettings.TwoFactorEnabled)
                {
                    string key = user.SecuritySettings.TwoFactorKey;

                    TimeAuthenticator ta = new TimeAuthenticator(usedCodeManager: _usedCodesManager);
                    bool isValid = ta.CheckCode(key, code, user);

                    if (isValid)
                    {
                        // the code was valid, let's log them in!
                        await SignInUser(user, returnUrl, rememberMe);

                        if (user.SecuritySettings.AllowTrustedDevices && rememberDevice)
                        {
                            // They want to remember the device, and have allow trusted devices on
                            var cookieOptions = UserHelper.CreateTrustedDeviceCookie(_config, user.Username, Request.Host.Host.GetDomain(), Request.IsLocal());
                            Response.Cookies.Append(Constants.TRUSTEDDEVICECOOKIE + "_" + username, cookieOptions.Item2, cookieOptions.Item1);

                            TrustedDevice device = new TrustedDevice();
                            device.UserId = user.UserId;
                            device.Name = (string.IsNullOrEmpty(deviceName)) ? "Unknown" : deviceName;
                            device.DateSeen = DateTime.Now;
                            device.Token = cookieOptions.Item2;

                            // Add the token
                            _dbContext.TrustedDevices.Add(device);
                            _dbContext.SaveChanges();
                        }

                        if (string.IsNullOrEmpty(returnUrl))
                            returnUrl = Request.Headers["Referer"].ToString();
                        return Json(new { result = returnUrl });
                    }
                    errorMessage = "Invalid Authentication Code" ;
                }
                errorMessage = "User does not have Two Factor Authentication enabled";
            }
            errorMessage = "User does not exist";

            // Raise the error event
            await _events.RaiseAsync(new UserLoginFailureEvent(username, errorMessage));
            return Json(new { error = errorMessage });
        }

        public async Task SignInUser(User user, string returnUrl, bool rememberMe)
        {
            // Sign In with Identity
            await _signInManager.SignInAsync(user, rememberMe);

            // Sign in via Identity Server
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.UserId.ToString(), user.Username));
        }

        public static async Task LogoutUser(ClaimsPrincipal user, HttpContext context, SignInManager<User> signInManager, IEventService eventService)
        {
            if (user?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await signInManager.SignOutAsync();

                // raise the logout event
                await eventService.RaiseAsync(new UserLogoutSuccessEvent(user.GetSubjectId(), user.GetDisplayName()));
            }
        }

    }
}