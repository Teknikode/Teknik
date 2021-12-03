using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Users.Models;
using Teknik.Configuration;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Utilities.Cryptography;
using MD5 = Teknik.Utilities.Cryptography.MD5;
using SHA256 = Teknik.Utilities.Cryptography.SHA256;
using SHA384 = Teknik.Utilities.Cryptography.SHA384;
using Teknik.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Teknik.MailService;
using Teknik.GitService;
using IdentityModel.Client;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Teknik.Utilities.Routing;

namespace Teknik.Areas.Users.Utility
{
    public static class UserHelper
    {
        #region Account Management
        public static List<string> GetReservedUsernames(Config config)
        {
            List<string> foundNames = new List<string>();
            if (config != null)
            {
                string path = config.UserConfig.ReservedUsernameDefinitionFile;
                if (File.Exists(path))
                {
                    string[] names = File.ReadAllLines(path);
                    foundNames = names.ToList();
                }
            }
            return foundNames;
        }

        public static bool UsernameReserved(Config config, string username)
        {
            // Load reserved usernames
            List<string> reserved = GetReservedUsernames(config);
            return (reserved.Exists(u => u.ToLower() == username.ToLower()));
        }

        public static bool ValidUsername(Config config, string username)
        {
            bool isValid = true;

            // Must be something there
            isValid &= !string.IsNullOrEmpty(username);

            // Is the format correct?
            Regex reg = new Regex(config.UserConfig.UsernameFilter);
            isValid &= reg.IsMatch(username);

            // Meets the min length?
            isValid &= (username.Length >= config.UserConfig.MinUsernameLength);

            // Meets the max length?
            isValid &= (username.Length <= config.UserConfig.MaxUsernameLength);

            return isValid;
        }

        public static async Task<bool> UsernameAvailable(TeknikEntities db, Config config, string username)
        {
            bool isAvailable = true;

            isAvailable &= ValidUsername(config, username);
            isAvailable &= !UsernameReserved(config, username);
            isAvailable &= !await IdentityHelper.UserExists(config, username);
            isAvailable &= !UserExists(db, username);
            isAvailable &= !UserEmailExists(config, GetUserEmailAddress(config, username));
            isAvailable &= !UserGitExists(config, username);

            return isAvailable;
        }

        public static async Task<DateTime> GetLastAccountActivity(TeknikEntities db, Config config, string username)
        {
            var userInfo = await IdentityHelper.GetIdentityUserInfo(config, username);
            return GetLastAccountActivity(db, config, username, userInfo);
        }

        public static DateTime GetLastAccountActivity(TeknikEntities db, Config config, string username, IdentityUserInfo userInfo)
        {
            try
            {
                DateTime lastActive = new DateTime(1900, 1, 1);

                if (UserEmailExists(config, GetUserEmailAddress(config, username)))
                {
                    DateTime emailLastActive = UserEmailLastActive(config, GetUserEmailAddress(config, username));
                    if (lastActive < emailLastActive)
                        lastActive = emailLastActive;
                }

                if (UserGitExists(config, username))
                {
                    DateTime gitLastActive = UserGitLastActive(config, username);
                    if (lastActive < gitLastActive)
                        lastActive = gitLastActive;
                }

                if (userInfo.LastSeen.HasValue)
                {
                    DateTime userLastActive = userInfo.LastSeen.Value;
                    if (lastActive < userLastActive)
                        lastActive = userLastActive;
                }

                return lastActive;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine last account activity.", ex);
            }
        }

        public static async Task CreateAccount(TeknikEntities db, Config config, IUrlHelper url, string username, string password, string recoveryEmail, string inviteCode)
        {
            try
            {
                var result = await IdentityHelper.CreateUser(config, username, password, recoveryEmail);
                if (result.Success)
                {
                    // Get the userId passed back
                    string userId = (string)result.Data;

                    // Create an Email Account
                    CreateUserEmail(config, GetUserEmailAddress(config, username), password);

                    // Disable the email account
                    DisableUserEmail(config, GetUserEmailAddress(config, username));

                    // Create a Git Account
                    CreateUserGit(config, username, password, userId);

                    // Add User
                    User newUser = CreateUser(db, config, username, inviteCode);

                    // If they have a recovery email, let's send a verification
                    if (!string.IsNullOrEmpty(recoveryEmail))
                    {
                        var token = await IdentityHelper.UpdateRecoveryEmail(config, username, recoveryEmail);
                        string resetUrl = url.SubRouteUrl("account", "User.ResetPassword", new { Username = username });
                        string verifyUrl = url.SubRouteUrl("account", "User.VerifyRecoveryEmail", new { Code = WebUtility.UrlEncode(token) });
                        SendRecoveryEmailVerification(config, username, recoveryEmail, resetUrl, verifyUrl);
                    }
                    return;
                }
                throw new Exception("Error creating account: " + result.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create account.", ex);
            }
        }

        public static void EditAccount(TeknikEntities db, Config config, User user)
        {
            try
            {
                // Update User
                EditUser(db, config, user);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit account.", ex);
            }
        }

        public static async Task ChangeAccountPassword(TeknikEntities db, Config config, string username, string currentPassword, string newPassword)
        {
            IdentityResult result = await IdentityHelper.UpdatePassword(config, username, currentPassword, newPassword);
            if (result.Success)
            {
                ChangeServicePasswords(db, config, username, newPassword);
            }
            else
            {
                throw new Exception(result.Message);
            }
        }

        public static async Task ResetAccountPassword(TeknikEntities db, Config config, string username, string token, string newPassword)
        {
            IdentityResult result = await IdentityHelper.ResetPassword(config, username, token, newPassword);
            if (result.Success)
            {
                ChangeServicePasswords(db, config, username, newPassword);
            }
            else
            {
                throw new Exception(result.Message);
            }
        }

        public static void ChangeServicePasswords(TeknikEntities db, Config config, string username, string newPassword)
        {
            try
            {
                // Make sure they have a git and email account before resetting their password
                string email = GetUserEmailAddress(config, username);
                if (config.EmailConfig.Enabled && UserEmailExists(config, email))
                {
                    // Change email password
                    EditUserEmailPassword(config, GetUserEmailAddress(config, username), newPassword);
                }

                if (config.GitConfig.Enabled && UserGitExists(config, username))
                {
                    // Update Git password
                    EditUserGitPassword(config, username, newPassword);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to change service password.", ex);
            }
        }

        public static async Task EditAccountType(TeknikEntities db, Config config, string username, AccountType type)
        {
            try
            {
                if (!UserExists(db, username))
                    throw new Exception($"The user provided does not exist: {username}");

                var result = await IdentityHelper.UpdateAccountType(config, username, type);

                if (result.Success)
                {

                    string email = GetUserEmailAddress(config, username);
                    // Add/Remove account type features depending on the type
                    switch (type)
                    {
                        case AccountType.Basic:
                            // Disable their email
                            DisableUserEmail(config, email);
                            break;
                        case AccountType.Premium:
                            // Enable their email account
                            EnableUserEmail(config, email);
                            break;
                    }
                }
                else
                {
                    throw new Exception($"Unable to edit the account type [{type}] for {username}: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to edit the account type [{type}] for: {username}", ex);
            }
        }

        public static async Task EditAccountStatus(TeknikEntities db, Config config, string username, AccountStatus status)
        {
            try
            {
                if (!UserExists(db, username))
                    throw new Exception($"The user provided does not exist: {username}");

                var result = await IdentityHelper.UpdateAccountStatus(config, username, status);

                if (result.Success)
                {
                    string email = GetUserEmailAddress(config, username);

                    // Add/Remove account type features depending on the type
                    switch (status)
                    {
                        case AccountStatus.Active:
                            // Enable Email
                            EnableUserEmail(config, email);
                            // Enable Git
                            EnableUserGit(config, username);
                            break;
                        case AccountStatus.Banned:
                            // Disable Email
                            DisableUserEmail(config, email);
                            // Disable Git
                            DisableUserGit(config, username);
                            break;
                    }
                }
                else
                {
                    throw new Exception($"Unable to edit the account status [{status}] for {username}: " + result.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to edit the account status [{status}] for: {username}", ex);
            }
        }

        public static async Task DeleteAccount(TeknikEntities db, Config config, User user)
        {
            try
            {
                string username = user.Username;

                // Delete identity account
                var result = await IdentityHelper.DeleteUser(config, username);

                if (result)
                {
                    // Delete User Account
                    DeleteUser(db, config, user);

                    // Delete Email Account
                    if (UserEmailExists(config, GetUserEmailAddress(config, username)))
                        DeleteUserEmail(config, GetUserEmailAddress(config, username));

                    // Delete Git Account
                    if (UserGitExists(config, username))
                        DeleteUserGit(config, username);
                }
                else
                {
                    throw new Exception("Unable to delete identity account.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete account.", ex);
            }
        }
        #endregion

        #region User Management
        public static User GetUser(TeknikEntities db, string username)
        {
            User user = db.Users
                .Include(u => u.UserSettings)
                .Include(u => u.BlogSettings)
                .Include(u => u.UploadSettings)
                .Include(u => u.BillingCustomer)
                .Where(b => b.Username == username).FirstOrDefault();

            return user;
        }

        public static bool UserExists(TeknikEntities db, string username)
        {
            User user = GetUser(db, username);
            if (user != null)
            {
                return true;
            }

            return false;
        }

        public static async Task<bool> UserPasswordCorrect(Config config, string username, string password)
        {
            try
            {
                return await IdentityHelper.CheckPassword(config, username, password);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine if password is correct.", ex);
            }
        }

        public static User CreateUser(TeknikEntities db, Config config, string username, string inviteCode)
        {
            try
            {
                User newUser = new User();
                newUser.Username = username;
                newUser.UserSettings = new UserSettings();
                newUser.BlogSettings = new BlogSettings();
                newUser.UploadSettings = new UploadSettings();

                // if they provided an invite code, let's assign them to it
                if (!string.IsNullOrEmpty(inviteCode))
                {
                    InviteCode code = db.InviteCodes.Where(c => c.Code == inviteCode).FirstOrDefault();
                    db.Entry(code).State = EntityState.Modified;

                    newUser.ClaimedInviteCode = code;
                }

                // Add User
                db.Users.Add(newUser);

                // Generate blog for the user
                var newBlog = new Blog.Models.Blog();
                newBlog.User = newUser;
                db.Blogs.Add(newBlog);

                // Save the changes
                db.SaveChanges();

                return newUser;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create user.", ex);
            }
        }

        public static void EditUser(TeknikEntities db, Config config, User user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to edit user {0}.", user.Username), ex);
            }
        }

        public static void DeleteUser(TeknikEntities db, Config config, User user)
        {
            try
            {
                // Update uploads
                List<Upload.Models.Upload> uploads = db.Uploads.Where(u => u.User.Username == user.Username).ToList();
                if (uploads.Any())
                {
                    foreach (Upload.Models.Upload upload in uploads)
                    {
                        upload.UserId = null;
                        db.Entry(upload).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                // Update pastes
                List<Paste.Models.Paste> pastes = db.Pastes.Where(u => u.User.Username == user.Username).ToList();
                if (pastes.Any())
                {
                    foreach (Paste.Models.Paste paste in pastes)
                    {
                        paste.UserId = null;
                        db.Entry(paste).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                // Update shortened urls
                List<ShortenedUrl> shortUrls = db.ShortenedUrls.Where(u => u.User.Username == user.Username).ToList();
                if (shortUrls.Any())
                {
                    foreach (ShortenedUrl shortUrl in shortUrls)
                    {
                        shortUrl.UserId = null;
                        db.Entry(shortUrl).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                // Update vaults
                List<Vault.Models.Vault> vaults = db.Vaults.Where(u => u.User.Username == user.Username).ToList();
                if (vaults.Any())
                {
                    foreach (Vault.Models.Vault vault in vaults)
                    {
                        vault.UserId = null;
                        db.Entry(vault).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                // Delete Blogs
                Blog.Models.Blog blog = db.Blogs.Where(u => u.User.Username == user.Username).FirstOrDefault();
                if (blog != null)
                {
                    db.Blogs.Remove(blog);
                    db.SaveChanges();
                }

                // Delete post comments
                List<BlogPostComment> postComments = db.BlogPostComments.Where(u => u.User.Username == user.Username).ToList();
                if (postComments.Any())
                {
                    foreach (BlogPostComment postComment in postComments)
                    {
                        db.BlogPostComments.Remove(postComment);
                    }
                    db.SaveChanges();
                }

                // Delete podcast comments
                List<Podcast.Models.PodcastComment> podComments = db.PodcastComments.Where(u => u.User.Username == user.Username).ToList();
                if (podComments.Any())
                {
                    foreach (Podcast.Models.PodcastComment podComment in podComments)
                    {
                        db.PodcastComments.Remove(podComment);
                    }
                    db.SaveChanges();
                }

                // Delete Owned Invite Codes
                List<InviteCode> ownedCodes = db.InviteCodes.Where(i => i.Owner.Username == user.Username).ToList();
                if (ownedCodes.Any())
                {
                    foreach (InviteCode code in ownedCodes)
                    {
                        db.InviteCodes.Remove(code);
                    }
                    db.SaveChanges();
                }

                // Delete Claimed Invite Code
                List<InviteCode> claimedCodes = db.InviteCodes.Where(i => i.ClaimedUser.Username == user.Username).ToList();
                if (claimedCodes.Any())
                {
                    foreach (InviteCode code in claimedCodes)
                    {
                        db.InviteCodes.Remove(code);
                    }
                    db.SaveChanges();
                }

                // Delete Auth Tokens
                //List<AuthToken> authTokens = db.AuthTokens.Where(t => t.User.UserId == user.UserId).ToList();
                //if (authTokens.Any())
                //{
                //    foreach (AuthToken authToken in authTokens)
                //    {
                //        db.AuthTokens.Remove(authToken);
                //    }
                //    db.SaveChanges();
                //}

                // Delete User
                db.Users.Remove(user);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to delete user {0}.", user.Username), ex);
            }
        }

        public static void SendRecoveryEmailVerification(Config config, string username, string email, string resetUrl, string verifyUrl)
        {
            SmtpClient client = new SmtpClient();
            client.Host = config.ContactConfig.EmailAccount.Host;
            client.Port = config.ContactConfig.EmailAccount.Port;
            client.EnableSsl = config.ContactConfig.EmailAccount.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential(config.ContactConfig.EmailAccount.Username, config.ContactConfig.EmailAccount.Password);
            client.Timeout = 5000;

            MailMessage mail = new MailMessage(new MailAddress(config.ContactConfig.EmailAccount.EmailAddress, "Teknik"), new MailAddress(email, username));
            mail.Subject = "Recovery Email Validation";
            mail.Body = string.Format(@"Hello {0},

You are recieving this email because you have specified this email address as your recovery email.  In the event that you forget your password, you can visit {1} and request a temporary password reset key be sent to this email.  You will then be able to reset and choose a new password.

In order to verify that you own this email, please click the following link or paste it into your browser: {2}  

If you recieved this email and you did not sign up for an account, please email us at {3} and ignore the verification link.

- Teknik", username, resetUrl, verifyUrl, config.SupportEmail);
            mail.BodyEncoding = UTF8Encoding.UTF8;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

            client.Send(mail);
        }

        public static void SendResetPasswordVerification(Config config, string username, string email, string resetUrl)
        {
            SmtpClient client = new SmtpClient();
            client.Host = config.ContactConfig.EmailAccount.Host;
            client.Port = config.ContactConfig.EmailAccount.Port;
            client.EnableSsl = config.ContactConfig.EmailAccount.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential(config.ContactConfig.EmailAccount.Username, config.ContactConfig.EmailAccount.Password);
            client.Timeout = 5000;

            MailMessage mail = new MailMessage(new MailAddress(config.ContactConfig.EmailAccount.EmailAddress, "Teknik"), new MailAddress(email, username));
            mail.Subject = "Password Reset Request";
            mail.Body = string.Format(@"Hello {0},

You are recieving this email because either you or someone has requested a password reset for your account and this email was specified as the recovery email.

To proceed in resetting your password, please click the following link or paste it into your browser: {1}  

If you recieved this email and you did not reset your password, you can ignore this email and email us at {2} to prevent it occuring again.

- Teknik", username, resetUrl, config.SupportEmail);
            mail.BodyEncoding = UTF8Encoding.UTF8;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

            client.Send(mail);
        }
        #endregion

        #region Email Management
        public static string GetUserEmailAddress(Config config, string username)
        {
            return string.Format("{0}@{1}", username, config.EmailConfig.Domain);
        }

        public static IMailService CreateMailService(Config config)
        {
            return new HMailService(
                config.EmailConfig.MailHost,
                config.EmailConfig.Username,
                config.EmailConfig.Password,
                config.EmailConfig.Domain,
                config.EmailConfig.CounterDatabase.Server,
                config.EmailConfig.CounterDatabase.Database,
                config.EmailConfig.CounterDatabase.Username,
                config.EmailConfig.CounterDatabase.Password,
                config.EmailConfig.CounterDatabase.Port
                );
        }

        public static bool UserEmailExists(Config config, string email)
        {
            // If Email Server is enabled
            if (config.EmailConfig.Enabled)
            {
                var svc = CreateMailService(config);
                return svc.AccountExists(email);
            }
            return false;
        }

        public static DateTime UserEmailLastActive(Config config, string email)
        {
            DateTime lastActive = new DateTime(1900, 1, 1);

            if (config.EmailConfig.Enabled)
            {
                var svc = CreateMailService(config);
                var lastEmail = svc.LastActive(email);
                if (lastActive < lastEmail)
                    lastActive = lastEmail;
            }
            return lastActive;
        }

        public static bool UserEmailEnabled(Config config, string email)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    return svc.Enabled(email);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to get email account status.", ex);
            }
            return false;
        }

        public static void CreateUserEmail(Config config, string email, string password)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.CreateAccount(email, password, config.EmailConfig.MaxSize);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to add email.", ex);
            }
        }

        public static void EnableUserEmail(Config config, string email)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.EnableAccount(email);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to enable email account.", ex);
            }
        }

        public static void DisableUserEmail(Config config, string email)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.DisableAccount(email);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to disable email account.", ex);
            }
        }

        public static void EditUserEmailPassword(Config config, string email, string password)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.EditPassword(email, password);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit email account password.", ex);
            }
        }

        public static void EditUserEmailMaxSize(Config config, string email, long size)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.EditMaxSize(email, size);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit email account mailbox size.", ex);
            }
        }

        public static void EditUserEmailMaxEmailsPerDay(Config config, string email, int maxPerDay)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.EditMaxEmailsPerDay(email, maxPerDay);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit email account mailbox size.", ex);
            }
        }

        public static void DeleteUserEmail(Config config, string email)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var svc = CreateMailService(config);
                    svc.DeleteAccount(email);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete email account.", ex);
            }
        }
        #endregion

        #region Git Management
        public static IGitService CreateGitService(Config config)
        {
            return new GiteaService(
                config.GitConfig.SourceId,
                config.GitConfig.Host,
                config.GitConfig.AccessToken,
                config.GitConfig.Database.Server,
                config.GitConfig.Database.Database,
                config.GitConfig.Database.Username,
                config.GitConfig.Database.Password,
                config.GitConfig.Database.Port
                );
        }

        public static bool UserGitExists(Config config, string username)
        {
            if (config.GitConfig.Enabled)
            {
                try
                {
                    var svc = CreateGitService(config);
                    return svc.AccountExists(username);
                }
                catch { }
            }
            return false;
        }

        public static DateTime UserGitLastActive(Config config, string username)
        {
            DateTime lastActive = new DateTime(1900, 1, 1);

            if (config.GitConfig.Enabled)
            {
                // Git user exists?
                if (!UserGitExists(config, username))
                {
                    throw new Exception($"Git User '{username}' does not exist.");
                }

                string email = GetUserEmailAddress(config, username);

                var svc = CreateGitService(config);
                DateTime tmpLast = svc.LastActive(email);
                if (lastActive < tmpLast)
                    lastActive = tmpLast;
            }
            return lastActive;
        }

        public static void CreateUserGit(Config config, string username, string password, string authId)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    string email = GetUserEmailAddress(config, username);

                    var svc = CreateGitService(config);
                    svc.CreateAccount(username, email, password, authId);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to add git account.", ex);
            }
        }

        public static void EditUserGitPassword(Config config, string username, string password)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Git user exists?
                    if (!UserGitExists(config, username))
                    {
                        throw new Exception($"Git User '{username}' does not exist.");
                    }

                    string email = GetUserEmailAddress(config, username);

                    var svc = CreateGitService(config);
                    svc.EditPassword(username, email, password);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit git account password.", ex);
            }
        }

        public static void EnableUserGit(Config config, string username)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Git user exists?
                    if (!UserGitExists(config, username))
                    {
                        throw new Exception($"Git User '{username}' does not exist.");
                    }

                    string email = GetUserEmailAddress(config, username);

                    var svc = CreateGitService(config);
                    svc.EnableAccount(username, email);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to enable git account.", ex);
            }
        }

        public static void DisableUserGit(Config config, string username)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Git user exists?
                    if (!UserGitExists(config, username))
                    {
                        throw new Exception($"Git User '{username}' does not exist.");
                    }

                    string email = GetUserEmailAddress(config, username);

                    var svc = CreateGitService(config);
                    svc.EnableAccount(username, email);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to disable git account.", ex);
            }
        }

        public static void DeleteUserGit(Config config, string username)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Git user exists?
                    if (!UserGitExists(config, username))
                    {
                        throw new Exception($"Git User '{username}' does not exist.");
                    }

                    var svc = CreateGitService(config);
                    svc.DeleteAccount(username);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete git account.", ex);
            }
        }

        public static void CreateUserGitTwoFactor(Config config, string username, string secret, int unixTime)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Git user exists?
                    if (!UserGitExists(config, username))
                    {
                        throw new Exception($"Git User '{username}' does not exist.");
                    }

                    // Generate the scratch token
                    string token = StringHelper.RandomString(8);

                    // Get the Encryption Key from the git secret key
                    byte[] keyBytes = MD5.Hash(Encoding.UTF8.GetBytes(config.GitConfig.SecretKey));

                    // Modify the input secret
                    byte[] secBytes = Encoding.UTF8.GetBytes(secret);

                    // Generate the encrypted secret using AES CGM
                    byte[] encValue = Aes128CFB.Encrypt(secBytes, keyBytes);
                    string finalSecret = Convert.ToBase64String(encValue);

                    // Create connection to the DB
                    Utilities.MysqlDatabase mySQL = new Utilities.MysqlDatabase(config.GitConfig.Database.Server, config.GitConfig.Database.Database, config.GitConfig.Database.Username, config.GitConfig.Database.Password, config.GitConfig.Database.Port);
                    mySQL.MysqlErrorEvent += (sender, s) =>
                    {
                        throw new Exception("Unable to edit git account two factor.  Mysql Exception: " + s);
                    };

                    // Get the user's UID
                    string email = GetUserEmailAddress(config, username);
                    string userSelect = @"SELECT gogs.user.id FROM gogs.user WHERE gogs.user.login_name = {0}";
                    var uid = mySQL.ScalarQuery(userSelect, new object[] { email });

                    // See if they have Two Factor already
                    string sqlSelect = @"SELECT tf.id 
                                FROM gogs.two_factor tf
                                LEFT JOIN gogs.user u ON u.id = tf.uid
                                WHERE u.login_name = {0}";
                    var result = mySQL.ScalarQuery(sqlSelect, new object[] { email });
                    
                    if (result != null)
                    {
                        // They have an entry!  Let's update it
                        string update = @"UPDATE gogs.two_factor tf SET tf.uid = {1}, tf.secret = {2}, tf.scratch_token = {3}, tf.updated_unix = {4} WHERE tf.id = {0}";

                        mySQL.Execute(update, new object[] { result, uid, finalSecret, token, unixTime });
                    }
                    else
                    {
                        // They need a new entry
                        string insert = @"INSERT INTO gogs.two_factor (uid, secret, scratch_token, created_unix, updated_unix) VALUES ({0}, {1}, {2}, {3}, {4})";

                        mySQL.Execute(insert, new object[] { uid, finalSecret, token, unixTime, 0 });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit git account two factor.", ex);
            }
        }

        public static void DeleteUserGitTwoFactor(Config config, string username)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Git user exists?
                    if (!UserGitExists(config, username))
                    {
                        throw new Exception($"Git User '{username}' does not exist.");
                    }

                    // Create connection to the DB
                    Utilities.MysqlDatabase mySQL = new Utilities.MysqlDatabase(config.GitConfig.Database.Server, config.GitConfig.Database.Database, config.GitConfig.Database.Username, config.GitConfig.Database.Password, config.GitConfig.Database.Port);

                    // Get the user's UID
                    string email = GetUserEmailAddress(config, username);

                    // See if they have Two Factor already
                    string deleteSql = @"DELETE tf.* 
                                FROM gogs.two_factor tf
                                LEFT JOIN gogs.user u ON u.id = tf.uid
                                WHERE u.login_name = {0}";
                    mySQL.Execute(deleteSql, new object[] { email });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete git account two factor.", ex);
            }
        }
        #endregion
    }
}
