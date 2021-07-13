using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Teknik.Configuration;
using Teknik.IdentityServer.Models;
using Teknik.IdentityServer.Models.Manage;
using Teknik.IdentityServer.Services;
using Teknik.Logging;
using Teknik.Utilities;

namespace Teknik.IdentityServer.Controllers
{
    [Authorize(Policy = "Internal", AuthenticationSchemes = "Bearer")]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ManageController : DefaultController
    {
        private const string _KeySeparator = ":";
        private const string _UserInfoCacheKey = "UserInfo";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMemoryCache _cache;

        public ManageController(
            ILogger<Logger> logger,
            Config config,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMemoryCache cache) : base(logger, config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = cache;
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
                return new JsonResult(new { success = true, data = identityUser.Id });
            }

            return new JsonResult(new { success = false, message = "Unable to create user.", identityErrors = result.Errors });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(DeleteUserModel model, [FromServices] ConfigurationDbContext configContext)
        {
            if (string.IsNullOrEmpty(model.Username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var foundUser = await _userManager.FindByNameAsync(model.Username);
            if (foundUser != null)
            {
                // Find this user's clients
                var lowerUsername = model.Username.ToLower();
                var foundClients = configContext.Clients
                                        .Select(c => new { Client = c, Username = c.Properties.FirstOrDefault(p => p.Key == "username").Value })
                                        .Where(c => c.Username.ToLower() == lowerUsername)
                                        .Select(c => c.Client);
                if (foundClients.Any())
                {
                    configContext.Clients.RemoveRange(foundClients);
                    configContext.SaveChanges();
                }

                var result = await _userManager.DeleteAsync(foundUser);
                if (result.Succeeded)
                {
                    RemoveCachedUser(model.Username);

                    return new JsonResult(new { success = true });
                }
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

            var foundUser = await GetCachedUser(username);
            if (foundUser != null)
            {
                var userJson = foundUser.ToJson();
                return new JsonResult(new { success = true, data = userJson });
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
                    // Remove the UserInfo Cache
                    RemoveCachedUser(model.Username);

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
                // Remove the UserInfo Cache
                RemoveCachedUser(model.Username);

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
                {
                    // Remove the UserInfo Cache
                    RemoveCachedUser(model.Username);

                    return new JsonResult(new { success = true });
                }
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
                {
                    // Remove the UserInfo Cache
                    RemoveCachedUser(model.Username);

                    return new JsonResult(new { success = true });
                }
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
                {
                    // Remove the UserInfo Cache
                    RemoveCachedUser(model.Username);

                    return new JsonResult(new { success = true });
                }
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
                // Remove the UserInfo Cache
                RemoveCachedUser(model.Username);

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
                        // Remove the UserInfo Cache
                        RemoveCachedUser(model.Username);

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
                {
                    // Remove the UserInfo Cache
                    RemoveCachedUser(model.Username);

                    return new JsonResult(new { success = true });
                }
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
                    // Remove the UserInfo Cache
                    RemoveCachedUser(model.Username);

                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(foundUser, 10);

                    return new JsonResult(new { success = true, data = recoveryCodes.ToArray() });
                }

                return new JsonResult(new { success = false, message = "Two-Factor Authentication is not enabled." });
            }

            return new JsonResult(new { success = false, message = "User does not exist." });
        }

        [HttpGet]
        public async Task<IActionResult> GetClient(string username, string clientId, [FromServices] IClientStore clientStore, [FromServices] ConfigurationDbContext configContext)
        {
            if (string.IsNullOrEmpty(username))
                return new JsonResult(new { success = false, message = "Username is required" });

            if (string.IsNullOrEmpty(clientId))
                return new JsonResult(new { success = false, message = "Client Id is required" });

            var lowerUsername = username.ToLower();
            var client = configContext.Clients
                                    .Select(c => new { Id = c.ClientId, Username = c.Properties.FirstOrDefault(p => p.Key == "username").Value })
                                    .FirstOrDefault(c => 
                                    c.Id == clientId &&
                                    c.Username.ToLower() == lowerUsername);
            if (client != null)
            {
                var foundClient = await clientStore.FindClientByIdAsync(client.Id);
                return new JsonResult(new { success = true, data = foundClient });
            }

            return new JsonResult(new { success = false, message = "Client does not exist." });
        }

        [HttpGet]
        public async Task<IActionResult> GetClients(string username, [FromServices] IClientStore clientStore, [FromServices] ConfigurationDbContext configContext)
        {
            if (string.IsNullOrEmpty(username))
                return new JsonResult(new { success = false, message = "Username is required" });

            var lowerUsername = username.ToLower();
            var foundClientIds = configContext.Clients
                                    .Select(c => new { Id = c.ClientId, Username = c.Properties.FirstOrDefault(p => p.Key == "username").Value })
                                    .Where(c => c.Username.ToLower() == lowerUsername);
            var clients = new List<IdentityServer4.Models.Client>();
            foreach (var client in foundClientIds)
            {
                var foundClient = await clientStore.FindClientByIdAsync(client.Id);
                if (foundClient != null)
                    clients.Add(foundClient);
            }

            return new JsonResult(new { success = true, data = clients });
        }

        [HttpPost]
        public IActionResult CreateClient(CreateClientModel model, [FromServices] ConfigurationDbContext configContext)
        {
            // Generate a unique client ID
            var clientId = StringHelper.RandomString(20, "abcdefghjkmnpqrstuvwxyz1234567890");
            while (configContext.Clients.Where(c => c.ClientId == clientId).FirstOrDefault() != null)
            {
                clientId = StringHelper.RandomString(20, "abcdefghjkmnpqrstuvwxyz1234567890");
            }

            var clientSecret = StringHelper.RandomString(40, "abcdefghjkmnpqrstuvwxyz1234567890");

            // Generate the origin for the callback
            Uri redirect = new Uri(model.CallbackUrl);
            string origin = redirect.Scheme + "://" + redirect.Host;

            var client = new IdentityServer4.Models.Client
            {
                Properties = new Dictionary<string, string>()
                {
                    { "username", model.Username }
                },
                ClientId = clientId,
                ClientName = model.Name,
                ClientUri = model.HomepageUrl,
                LogoUri = model.LogoUrl,

                AllowedGrantTypes = model.AllowedGrants,
                AllowedScopes = model.AllowedScopes,

                ClientSecrets =
                {
                    new IdentityServer4.Models.Secret(clientSecret.Sha256())
                },

                RedirectUris =
                {
                    model.CallbackUrl
                },

                AllowedCorsOrigins =
                {
                    origin
                },

                RequireConsent = true,
                AllowOfflineAccess = true
            };

            configContext.Clients.Add(client.ToEntity());
            configContext.SaveChanges();

            return new JsonResult(new { success = true, data = new { id = clientId, secret = clientSecret } });
        }

        [HttpPost]
        public IActionResult EditClient(EditClientModel model, [FromServices] ConfigurationDbContext configContext)
        {
            // Validate it's an actual client
            var foundClient = configContext.Clients.Where(c => c.ClientId == model.ClientId).FirstOrDefault();
            if (foundClient != null)
            {
                foundClient.ClientName = model.Name;
                foundClient.ClientUri = model.HomepageUrl;
                foundClient.LogoUri = model.LogoUrl;
                foundClient.Updated = DateTime.Now;
                configContext.Entry(foundClient).State = EntityState.Modified;

                // Update the redirect URL for this client
                var results = configContext.Set<ClientRedirectUri>().Where(c => c.ClientId == foundClient.Id).ToList();
                if (results != null)
                {
                    configContext.RemoveRange(results);
                }
                var newUri = new ClientRedirectUri();
                newUri.Client = foundClient;
                newUri.ClientId = foundClient.Id;
                newUri.RedirectUri = model.CallbackUrl;
                configContext.Add(newUri);

                // Generate the origin for the callback
                Uri redirect = new Uri(model.CallbackUrl);
                string origin = redirect.Scheme + "://" + redirect.Host;

                // Update the allowed origin for this client
                var corsOrigins = configContext.Set<ClientCorsOrigin>().Where(c => c.ClientId == foundClient.Id).ToList();
                if (corsOrigins != null)
                {
                    configContext.RemoveRange(corsOrigins);
                }
                var newOrigin = new ClientCorsOrigin();
                newOrigin.Client = foundClient;
                newOrigin.ClientId = foundClient.Id;
                newOrigin.Origin = origin;
                configContext.Add(newUri);

                // Update their allowed grants
                var curGrants = configContext.Set<ClientGrantType>().Where(c => c.ClientId == foundClient.Id).ToList();
                if (curGrants != null)
                {
                    configContext.RemoveRange(curGrants);
                }
                foreach (var grantType in model.AllowedGrants)
                {
                    var newGrant = new ClientGrantType();
                    newGrant.Client = foundClient;
                    newGrant.ClientId = foundClient.Id;
                    newGrant.GrantType = grantType;
                    configContext.Add(newGrant);
                }

                // Update their allowed scopes
                var curScopes = configContext.Set<ClientScope>().Where(c => c.ClientId == foundClient.Id).ToList();
                if (curScopes != null)
                {
                    configContext.RemoveRange(curScopes);
                }
                foreach (var scope in model.AllowedScopes)
                {
                    var newScope = new ClientScope();
                    newScope.Client = foundClient;
                    newScope.ClientId = foundClient.Id;
                    newScope.Scope = scope;
                    configContext.Add(newScope);
                }

                // Save all the changed
                configContext.SaveChanges();

                // Clear the client cache
                RemoveCachedClient(model.ClientId);

                return new JsonResult(new { success = true });
            }

            return new JsonResult(new { success = false, message = "Client does not exist." });
        }

        [HttpPost]
        public IActionResult DeleteClient(DeleteClientModel model, [FromServices] ConfigurationDbContext configContext)
        {            
            var foundClient = configContext.Clients.Where(c => c.ClientId == model.ClientId).FirstOrDefault();
            if (foundClient != null)
            {
                configContext.Clients.Remove(foundClient);
                configContext.SaveChanges();

                // Clear the client cache
                RemoveCachedClient(model.ClientId);

                return new JsonResult(new { success = true });
            }

            return new JsonResult(new { success = false, message = "Client does not exist." });
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

        private async Task<ApplicationUser> GetCachedUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            // Check the cache
            string cacheKey = GetKey<ApplicationUser>(username);
            ApplicationUser foundUser;
            if (!_cache.TryGetValue(cacheKey, out foundUser))
            {
                foundUser = await _userManager.FindByNameAsync(username);
                if (foundUser != null)
                {
                    _cache.AddToCache(cacheKey, foundUser, new TimeSpan(1, 0, 0));
                }
            }

            return foundUser;
        }

        private void RemoveCachedUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            string cacheKey = GetKey<ApplicationUser>(username);
            _cache.Remove(cacheKey);
        }

        private void RemoveCachedClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException("clientId");

            string key = GetKey<IdentityServer4.Models.Client>(clientId);
            _cache.Remove(key);
        }

        private string GetKey<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return typeof(T).FullName + _KeySeparator + key;
        }
    }
}