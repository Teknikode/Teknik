using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.IdentityServer.Models;
using Teknik.IdentityServer.Models.Manage;
using Teknik.Logging;

namespace Teknik.IdentityServer.Controllers
{
    [Authorize(Policy = "Internal", AuthenticationSchemes = "Bearer")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ManageController : DefaultController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ManageController(
            ILogger<Logger> logger,
            Config config,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) : base(logger, config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(NewUserModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });
            if (string.IsNullOrEmpty(model.Password))
                return new JsonResult(new { success = false, message = "Password is required" });

            var identityUser = new ApplicationUser(model.Username)
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.Username,
                AccountStatus = model.AccountStatus,
                AccountType = model.AccountType,
                Email = model.RecoveryEmail,
                EmailConfirmed = model.RecoveryVerified,
                PGPPublicKey = model.PGPPublicKey
            };
            var result = await _userManager.CreateAsync(identityUser, model.Password);
            if (result.Succeeded)
            {
                return new JsonResult(new { success = true });
            }

            return new JsonResult(new { success = false, message = "Unable to create user.", identityErrors = result.Errors });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(DeleteUserModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                var result = await _userManager.DeleteAsync(foundUser);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to delete user.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpGet]
        public async Task<IActionResult> UserExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(username);
            return new JsonResult(new { success = true, data = foundUser != null });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string username)
        {
            if (string.IsNullOrEmpty(username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser != null)
            {
                return new JsonResult(new { success = true, data = foundUser.ToJson() });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> CheckPassword(CheckPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });
            if (string.IsNullOrEmpty(model.Password))
                return new JsonResult(new { success = false, message = "Password is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                bool valid = await _userManager.CheckPasswordAsync(foundUser, model.Password);
                return new JsonResult(new { success = true, data = valid });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePasswordResetToken(GeneratePasswordResetTokenModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(foundUser);
                return new JsonResult(new { success = true, data = token });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });
            if (string.IsNullOrEmpty(model.Token))
                return new JsonResult(new { success = false, message = "Token is required" });
            if (string.IsNullOrEmpty(model.Password))
                return new JsonResult(new { success = false, message = "Password is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                var result = await _userManager.ResetPasswordAsync(foundUser, model.Token, model.Password);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to reset password.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });
            if (string.IsNullOrEmpty(model.CurrentPassword))
                return new JsonResult(new { success = false, message = "Current Password is required" });
            if (string.IsNullOrEmpty(model.NewPassword))
                return new JsonResult(new { success = false, message = "New Password is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                var result = await _userManager.ChangePasswordAsync(foundUser, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to update password.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmail(UpdateEmailModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                var result = await _userManager.SetEmailAsync(foundUser, model.Email);
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(foundUser);
                    return new JsonResult(new { success = true, data = token });
                }
                else
                    return new JsonResult(new { success = false, message = "Unable to update email address.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });
            if (string.IsNullOrEmpty(model.Token))
                return new JsonResult(new { success = false, message = "Token is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                var result = await _userManager.ConfirmEmailAsync(foundUser, model.Token);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to verify email address.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccountStatus(UpdateAccountStatusModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                foundUser.AccountStatus = model.AccountStatus;

                var result = await _userManager.UpdateAsync(foundUser);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to update account status.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccountType(UpdateAccountTypeModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                foundUser.AccountType = model.AccountType;

                var result = await _userManager.UpdateAsync(foundUser);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to update account type.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePGPPublicKey(UpdatePGPPublicKeyModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                foundUser.PGPPublicKey = model.PGPPublicKey;

                var result = await _userManager.UpdateAsync(foundUser);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to update pgp public key.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpGet]
        public async Task<IActionResult> Get2FAKey(string username)
        {
            if (string.IsNullOrEmpty(username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(username);
            if (foundUser != null)
            {
                string unformattedKey = await _userManager.GetAuthenticatorKeyAsync(foundUser);

                return new JsonResult(new { success = true, data = FormatKey(unformattedKey) });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> Reset2FAKey(Reset2FAKeyModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                await _userManager.ResetAuthenticatorKeyAsync(foundUser);
                string unformattedKey = await _userManager.GetAuthenticatorKeyAsync(foundUser);

                return new JsonResult(new { success = true, data = FormatKey(unformattedKey) });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> Enable2FA(Enable2FAModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });
            if (string.IsNullOrEmpty(model.Code))
                return new JsonResult(new { success = false, message = "Code is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                // Strip spaces and hypens
                var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

                var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                    foundUser, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

                if (is2faTokenValid)
                {
                    var result = await _userManager.SetTwoFactorEnabledAsync(foundUser, true);
                    if (result.Succeeded)
                    {
                        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(foundUser, 10);
                        return new JsonResult(new { success = true, data = recoveryCodes.ToArray() });
                    }
                    else
                        return new JsonResult(new { success = false, message = "Unable to set Two-Factor Authentication.", identityErrors = result.Errors });
                }

                return new JsonResult(new { success = false, message = "Verification code is invalid." });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> Disable2FA(Disable2FAModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                var result = await _userManager.SetTwoFactorEnabledAsync(foundUser, false);
                if (result.Succeeded)
                    return new JsonResult(new { success = true });
                else
                    return new JsonResult(new { success = false, message = "Unable to disable Two-Factor Authentication.", identityErrors = result.Errors });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateRecoveryCodes(GenerateRecoveryCodesModel model)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                if (foundUser.TwoFactorEnabled)
                {
                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(foundUser, 10);

                    return new JsonResult(new { success = true, data = recoveryCodes.ToArray() });
                }

                return new JsonResult(new { success = false, message = "Two-Factor Authentication is not enabled." });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
    }
}