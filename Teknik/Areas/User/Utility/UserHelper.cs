using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Web.Security;
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

        public static bool UsernameAvailable(TeknikEntities db, Config config, string username)
        {
            bool isAvailable = true;

            isAvailable &= ValidUsername(config, username);
            isAvailable &= !UsernameReserved(config, username);
            isAvailable &= !UserExists(db, username);
            isAvailable &= !UserEmailExists(config, GetUserEmailAddress(config, username));
            isAvailable &= !UserGitExists(config, username);

            return isAvailable;
        }

        public static DateTime GetLastAccountActivity(TeknikEntities db, Config config, User user)
        {
            try
            {
                DateTime lastActive = new DateTime(1900, 1, 1);

                DateTime emailLastActive = UserEmailLastActive(config, GetUserEmailAddress(config, user.Username));
                if (lastActive < emailLastActive)
                    lastActive = emailLastActive;

                DateTime gitLastActive = UserGitLastActive(config, user.Username);
                if (lastActive < gitLastActive)
                    lastActive = gitLastActive;

                DateTime userLastActive = UserLastActive(db, config, user);
                if (lastActive < userLastActive)
                    lastActive = userLastActive;

                return lastActive;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine last account activity.", ex);
            }
        }

        public static string GeneratePassword(Config config, User user, string password)
        {
            try
            {
                string username = user.Username.ToLower();
                if (user.Transfers.ToList().Exists(t => t.Type == TransferTypes.CaseSensitivePassword))
                {
                    username = user.Username;
                }
                byte[] hashBytes = SHA384.Hash(username, password);
                string hash = hashBytes.ToHex();

                if (user.Transfers.ToList().Exists(t => t.Type == TransferTypes.ASCIIPassword))
                {
                    hash = Encoding.ASCII.GetString(hashBytes);
                }

                if (user.Transfers.ToList().Exists(t => t.Type == TransferTypes.Sha256Password))
                {
                    hash = SHA256.Hash(password, config.Salt1, config.Salt2);
                }

                return hash;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to generate password.", ex);
            }
        }

        public static string GenerateAuthToken(TeknikEntities db, string username)
        {
            try
            {
                bool validToken = false;
                string token = string.Empty;
                while (!validToken)
                {
                    username = username.ToLower();
                    byte[] hashBytes = SHA384.Hash(username, StringHelper.RandomString(24));
                    token = hashBytes.ToHex();

                    // Make sure it isn't a duplicate
                    string hashedToken = SHA256.Hash(token);
                    if (!db.AuthTokens.Where(t => t.HashedToken == hashedToken).Any())
                    {
                        validToken = true;
                    }
                }

                return token;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to generate user auth token.", ex);
            }
        }

        public static void AddAccount(TeknikEntities db, Config config, User user, string password)
        {
            try
            {
                // Create an Email Account
                AddUserEmail(config, GetUserEmailAddress(config, user.Username), password);

                // Create a Git Account
                AddUserGit(config, user.Username, password);

                // Add User
                AddUser(db, config, user, password);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create account.", ex);
            }
        }

        public static void EditAccount(TeknikEntities db, Config config, User user, bool changePass, string password)
        {
            try
            {
                // Changing Password?
                if (changePass)
                {
                    // Make sure they have a git and email account before resetting their password
                    string email = GetUserEmailAddress(config, user.Username);
                    if (config.EmailConfig.Enabled && !UserEmailExists(config, email))
                    {
                        AddUserEmail(config, email, password);
                    }

                    if (config.GitConfig.Enabled && !UserGitExists(config, user.Username))
                    {
                        AddUserGit(config, user.Username, password);
                    }

                    // Change email password
                    EditUserEmailPassword(config, GetUserEmailAddress(config, user.Username), password);

                    // Update Git password
                    EditUserGitPassword(config, user.Username, password);
                }
                // Update User
                EditUser(db, config, user, changePass, password);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit account.", ex);
            }
        }

        public static void EditAccountType(TeknikEntities db, Config config, string username, AccountType type)
        {
            try
            {
                if (!UserExists(db, username))
                    throw new Exception($"The user provided does not exist: {username}");

                // Get the user to edit
                User user = GetUser(db, username);

                string email = GetUserEmailAddress(config, username);

                // Edit the user type
                user.AccountType = type;
                EditUser(db, config, user);

                // Add/Remove account type features depending on the type
                switch (type)
                {
                    case AccountType.Basic:
                        // Set the email size to 1GB
                        EditUserEmailMaxSize(config, email, config.EmailConfig.MaxSize);

                        // Set the email max/day to 100
                        EditUserEmailMaxEmailsPerDay(config, email, 100);
                        break;
                    case AccountType.Premium:
                        // Set the email size to 5GB
                        EditUserEmailMaxSize(config, email, 5000);

                        // Set the email max/day to infinite (-1)
                        EditUserEmailMaxEmailsPerDay(config, email, -1);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to edit the account type [{type}] for: {username}", ex);
            }
        }

        public static void DeleteAccount(TeknikEntities db, Config config, User user)
        {
            try
            {
                // Delete Email Account
                if (UserEmailExists(config, GetUserEmailAddress(config, user.Username)))
                    DeleteUserEmail(config, GetUserEmailAddress(config, user.Username));

                // Delete Git Account
                if (UserGitExists(config, user.Username))
                    DeleteUserGit(config, user.Username);

                // Delete User Account
                DeleteUser(db, config, user);
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
            User user = db.Users.Where(b => b.Username == username).FirstOrDefault();
            if (user != null)
            {
                user.UserSettings = db.UserSettings.Find(user.UserId);
                user.SecuritySettings = db.SecuritySettings.Find(user.UserId);
                user.BlogSettings = db.BlogSettings.Find(user.UserId);
                user.UploadSettings = db.UploadSettings.Find(user.UserId);
            }

            return user;
        }

        public static User GetUserFromToken(TeknikEntities db, string username, string token)
        {
            if (token != null && !string.IsNullOrEmpty(username))
            {
                string hashedToken = SHA256.Hash(token);
                return db.Users.FirstOrDefault(u => u.AuthTokens.Select(a => a.HashedToken).Contains(hashedToken) && u.Username == username);
            }
            return null;
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

        public static DateTime UserLastActive(TeknikEntities db, Config config, User user)
        {
            try
            {
                DateTime lastActive = new DateTime(1900, 1, 1);

                if (lastActive < user.LastSeen)
                    lastActive = user.LastSeen;

                return lastActive;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine last user activity.", ex);
            }
        }

        public static void UpdateTokenLastUsed(TeknikEntities db, string username, string token, DateTime lastUsed)
        {
            User foundUser = GetUser(db, username);
            if (foundUser != null)
            {
                string hashedToken = SHA256.Hash(token);
                List<AuthToken> tokens = foundUser.AuthTokens.Where(t => t.HashedToken == hashedToken).ToList();
                if (tokens != null)
                {
                    foreach (AuthToken foundToken in tokens)
                    {
                        foundToken.LastDateUsed = lastUsed;
                        db.Entry(foundToken).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
        }

        public static bool UserPasswordCorrect(TeknikEntities db, Config config, User user, string password)
        {
            try
            {
                string hash = GeneratePassword(config, user, password);
                return db.Users.Any(b => b.Username == user.Username && b.HashedPassword == hash);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to determine if password is correct.", ex);
            }
        }

        public static bool UserTokenCorrect(TeknikEntities db, string username, string token)
        {
            User foundUser = GetUserFromToken(db, username, token);
            if (foundUser != null)
            {
                return true;
            }
            return false;
        }

        public static bool UserHasRoles(TeknikEntities db, User user, params string[] roles)
        {
            bool hasRole = true;
            if (user != null)
            {
                // Check if they have the role specified
                if (roles.Any())
                {
                    foreach (string role in roles)
                    {
                        if (!string.IsNullOrEmpty(role))
                        {
                            if (user.Groups.Where(g => g.Roles.Where(r => role == r.Name).Any()).Any())
                            {
                                // They have the role!
                                return true;
                            }
                            else
                            {
                                // They don't have this role, so let's reset the hasRole
                                hasRole = false;
                            }
                        }
                        else
                        {
                            // Only set this if we haven't failed once already
                            hasRole &= true;
                        }
                    }
                }
                else
                {
                    // No roles to check, so they pass!
                    return true;
                }
            }
            else
            {
                hasRole = false;
            }
            return hasRole;
        }

        public static void TransferUser(TeknikEntities db, Config config, User user, string password)
        {
            try
            {
                List<TransferType> transfers = user.Transfers.ToList();
                for (int i = 0; i < transfers.Count; i++)
                {
                    TransferType transfer = transfers[i];
                    switch (transfer.Type)
                    {
                        case TransferTypes.Sha256Password:
                        case TransferTypes.CaseSensitivePassword:
                        case TransferTypes.ASCIIPassword:
                            user.HashedPassword = SHA384.Hash(user.Username.ToLower(), password).ToHex();
                            break;
                        default:
                            break;
                    }
                    user.Transfers.Remove(transfer);
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to transfer user info.", ex);
            }
        }

        public static void AddUser(TeknikEntities db, Config config, User user, string password)
        {
            try
            {
                // Add User
                user.HashedPassword = GeneratePassword(config, user, password);
                db.Users.Add(user);
                db.SaveChanges();

                // Generate blog for the user
                var newBlog = db.Blogs.Create();
                newBlog.UserId = user.UserId;
                db.Blogs.Add(newBlog);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create user.", ex);
            }
        }

        public static void EditUser(TeknikEntities db, Config config, User user)
        {
            EditUser(db, config, user, false, string.Empty);
        }

        public static void EditUser(TeknikEntities db, Config config, User user, bool changePass, string password)
        {
            try
            {
                // Changing Password?
                if (changePass)
                {
                    // Update User password
                    user.HashedPassword = SHA384.Hash(user.Username.ToLower(), password).ToHex();

                    // Remove any password transfer items for the account
                    for (int i = 0; i < user.Transfers.Count; i++)
                    {
                        TransferType type = user.Transfers.ToList()[i];
                        if (type.Type == TransferTypes.ASCIIPassword || type.Type == TransferTypes.CaseSensitivePassword || type.Type == TransferTypes.Sha256Password)
                        {
                            user.Transfers.Remove(type);
                            i--;
                        }
                    }
                }
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
                if (uploads != null)
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
                if (pastes != null)
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
                if (shortUrls != null)
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
                if (vaults != null)
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
                List<BlogPostComment> postComments = db.BlogComments.Where(u => u.User.Username == user.Username).ToList();
                if (postComments != null)
                {
                    foreach (BlogPostComment postComment in postComments)
                    {
                        db.BlogComments.Remove(postComment);
                    }
                    db.SaveChanges();
                }

                // Delete podcast comments
                List<Podcast.Models.PodcastComment> podComments = db.PodcastComments.Where(u => u.User.Username == user.Username).ToList();
                if (podComments != null)
                {
                    foreach (Podcast.Models.PodcastComment podComment in podComments)
                    {
                        db.PodcastComments.Remove(podComment);
                    }
                    db.SaveChanges();
                }

                // Delete Recovery Email Verifications
                List<RecoveryEmailVerification> verCodes = db.RecoveryEmailVerifications.Where(r => r.User.Username == user.Username).ToList();
                if (verCodes != null)
                {
                    foreach (RecoveryEmailVerification verCode in verCodes)
                    {
                        db.RecoveryEmailVerifications.Remove(verCode);
                    }
                    db.SaveChanges();
                }

                // Delete Password Reset Verifications 
                List<ResetPasswordVerification> verPass = db.ResetPasswordVerifications.Where(r => r.User.Username == user.Username).ToList();
                if (verPass != null)
                {
                    foreach (ResetPasswordVerification ver in verPass)
                    {
                        db.ResetPasswordVerifications.Remove(ver);
                    }
                    db.SaveChanges();
                }

                // Delete User
                db.Users.Remove(user);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to delete user {0}.", user.Username), ex);
            }
        }

        public static string CreateRecoveryEmailVerification(TeknikEntities db, Config config, User user)
        {
            // Check to see if there already is a verification code for the user
            List<RecoveryEmailVerification> verCodes = db.RecoveryEmailVerifications.Where(r => r.User.Username == user.Username).ToList();
            if (verCodes != null && verCodes.Any())
            {
                foreach (RecoveryEmailVerification verCode in verCodes)
                {
                    db.RecoveryEmailVerifications.Remove(verCode);
                }
            }

            // Create a new verification code and add it
            string verifyCode = StringHelper.RandomString(24);
            RecoveryEmailVerification ver = new RecoveryEmailVerification();
            ver.UserId = user.UserId;
            ver.Code = verifyCode;
            ver.DateCreated = DateTime.Now;
            db.RecoveryEmailVerifications.Add(ver);
            db.SaveChanges();

            return verifyCode;
        }

        public static void SendRecoveryEmailVerification(Config config, string username, string email, string resetUrl, string verifyUrl)
        {
            SmtpClient client = new SmtpClient();
            client.Host = config.ContactConfig.EmailAccount.Host;
            client.Port = config.ContactConfig.EmailAccount.Port;
            client.EnableSsl = config.ContactConfig.EmailAccount.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.Credentials = new NetworkCredential(config.ContactConfig.EmailAccount.Username, config.ContactConfig.EmailAccount.Password);
            client.Timeout = 5000;

            MailMessage mail = new MailMessage(config.ContactConfig.EmailAccount.EmailAddress, email);
            mail.Subject = "Recovery Email Validation";
            mail.Body = string.Format(@"Hello {0},

Welcome to Teknik!  

You are recieving this email because you have specified this email address as your recovery email.  In the event that you forget your password, you can visit {1} and request a temporary password reset key be sent to this email.  You will then be able to reset and choose a new password.

In order to verify that you own this email, please click the following link or paste it into your browser: {2}  

If you recieved this email and you did not sign up for an account, please email us at {3} and ignore the verification link.

Thank you and enjoy!

- Teknik Administration", username, resetUrl, verifyUrl, config.SupportEmail);
            mail.BodyEncoding = UTF8Encoding.UTF8;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

            client.Send(mail);
        }

        public static bool VerifyRecoveryEmail(TeknikEntities db, Config config, string username, string code)
        {
            User user = GetUser(db, username);
            RecoveryEmailVerification verCode = db.RecoveryEmailVerifications.Where(r => r.User.Username == username && r.Code == code).FirstOrDefault();
            if (verCode != null)
            {
                // We have a match, so clear out the verifications for that user
                List<RecoveryEmailVerification> verCodes = db.RecoveryEmailVerifications.Where(r => r.User.Username == username).ToList();
                if (verCodes != null && verCodes.Any())
                {
                    foreach (RecoveryEmailVerification ver in verCodes)
                    {
                        db.RecoveryEmailVerifications.Remove(ver);
                    }
                }
                // Update the user
                user.SecuritySettings.RecoveryVerified = true;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return true;
            }
            return false;
        }

        public static string CreateResetPasswordVerification(TeknikEntities db, Config config, User user)
        {
            // Check to see if there already is a verification code for the user
            List<ResetPasswordVerification> verCodes = db.ResetPasswordVerifications.Where(r => r.User.Username == user.Username).ToList();
            if (verCodes != null && verCodes.Any())
            {
                foreach (ResetPasswordVerification verCode in verCodes)
                {
                    db.ResetPasswordVerifications.Remove(verCode);
                }
            }

            // Create a new verification code and add it
            string verifyCode = StringHelper.RandomString(24);
            ResetPasswordVerification ver = new ResetPasswordVerification();
            ver.UserId = user.UserId;
            ver.Code = verifyCode;
            ver.DateCreated = DateTime.Now;
            db.ResetPasswordVerifications.Add(ver);
            db.SaveChanges();

            return verifyCode;
        }

        public static void SendResetPasswordVerification(Config config, string username, string email, string resetUrl)
        {
            SmtpClient client = new SmtpClient();
            client.Host = config.ContactConfig.EmailAccount.Host;
            client.Port = config.ContactConfig.EmailAccount.Port;
            client.EnableSsl = config.ContactConfig.EmailAccount.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.Credentials = new NetworkCredential(config.ContactConfig.EmailAccount.Username, config.ContactConfig.EmailAccount.Password);
            client.Timeout = 5000;

            MailMessage mail = new MailMessage(config.ContactConfig.EmailAccount.EmailAddress, email);
            mail.Subject = "Password Reset Request";
            mail.Body = string.Format(@"Hello {0},

You are recieving this email because either you or someone has requested a password reset for your account and this email was specified as the recovery email.

To proceed in resetting your password, please click the following link or paste it into your browser: {1}  

If you recieved this email and you did not reset your password, you can ignore this email and email us at {2} to prevent it occuring again.

- Teknik Administration", username, resetUrl, config.SupportEmail);
            mail.BodyEncoding = UTF8Encoding.UTF8;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

            client.Send(mail);
        }

        public static bool VerifyResetPassword(TeknikEntities db, Config config, string username, string code)
        {
            User user = GetUser(db, username);
            ResetPasswordVerification verCode = db.ResetPasswordVerifications.Where(r => r.User.Username == username && r.Code == code).FirstOrDefault();
            if (verCode != null)
            {
                // We have a match, so clear out the verifications for that user
                List<ResetPasswordVerification> verCodes = db.ResetPasswordVerifications.Where(r => r.User.Username == username).ToList();
                if (verCodes != null && verCodes.Any())
                {
                    foreach (ResetPasswordVerification ver in verCodes)
                    {
                        db.ResetPasswordVerifications.Remove(ver);
                    }
                }
                db.SaveChanges();

                return true;
            }
            return false;
        }
        #endregion

        #region Email Management
        public static string GetUserEmailAddress(Config config, string username)
        {
            return string.Format("{0}@{1}", username, config.EmailConfig.Domain);
        }

        public static bool UserEmailExists(Config config, string email)
        {
            // If Email Server is enabled
            if (config.EmailConfig.Enabled)
            {
                // Connect to hmailserver COM
                var app = new hMailServer.Application();
                app.Connect();
                app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);

                try
                {
                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var account = domain.Accounts.ItemByAddress[email];
                    // We didn't error out, so the email exists
                    return true;
                }
                catch { }
            }
            return false;
        }

        public static DateTime UserEmailLastActive(Config config, string email)
        {
            DateTime lastActive = new DateTime(1900, 1, 1);

            if (config.EmailConfig.Enabled)
            {
                var app = new hMailServer.Application();
                app.Connect();
                app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);

                try
                {
                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var account = domain.Accounts.ItemByAddress[email];
                    DateTime lastEmail = (DateTime)account.LastLogonTime;
                    if (lastActive < lastEmail)
                        lastActive = lastEmail;
                }
                catch { }
            }
            return lastActive;
        }

        public static void AddUserEmail(Config config, string email, string password)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    // Connect to hmailserver COM
                    var app = new hMailServer.Application();
                    app.Connect();
                    app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);

                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var newAccount = domain.Accounts.Add();
                    newAccount.Address = email;
                    newAccount.Password = password;
                    newAccount.Active = true;
                    newAccount.MaxSize = config.EmailConfig.MaxSize;

                    newAccount.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to add email.", ex);
            }
        }

        public static void EditUserEmailPassword(Config config, string email, string password)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var app = new hMailServer.Application();
                    app.Connect();
                    app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);
                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var account = domain.Accounts.ItemByAddress[email];
                    account.Password = password;
                    account.Save();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit email account password.", ex);
            }
        }

        public static void EditUserEmailMaxSize(Config config, string email, int size)
        {
            try
            {
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    var app = new hMailServer.Application();
                    app.Connect();
                    app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);
                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var account = domain.Accounts.ItemByAddress[email];
                    account.MaxSize = size;
                    account.Save();
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
                    // We need to check the actual git database
                    MysqlDatabase mySQL = new MysqlDatabase(config.EmailConfig.CounterDatabase.Server, config.EmailConfig.CounterDatabase.Database, config.EmailConfig.CounterDatabase.Username, config.EmailConfig.CounterDatabase.Password, config.EmailConfig.CounterDatabase.Port);
                    string sql = @"INSERT INTO mailcounter.counts (qname, lastdate, qlimit, count) VALUES ({1}, NOW(), {0}, 0)
                                    ON DUPLICATE KEY UPDATE qlimit = {0}";
                    mySQL.Execute(sql, new object[] { maxPerDay, email });
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
                    var app = new hMailServer.Application();
                    app.Connect();
                    app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);
                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var account = domain.Accounts.ItemByAddress[email];
                    if (account != null)
                    {
                        account.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete email account.", ex);
            }
        }
        #endregion

        #region Git Management
        public static bool UserGitExists(Config config, string username)
        {
            if (config.GitConfig.Enabled)
            {
                try
                {
                    Uri baseUri = new Uri(config.GitConfig.Host);
                    Uri finalUri = new Uri(baseUri, "api/v1/users/" + username + "?token=" + config.GitConfig.AccessToken);
                    WebRequest request = WebRequest.Create(finalUri);
                    request.Method = "GET";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
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
                // We need to check the actual git database
                MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database.Server, config.GitConfig.Database.Database, config.GitConfig.Database.Username, config.GitConfig.Database.Password, config.GitConfig.Database.Port);
                string sql = @"SELECT 
	                                CASE
		                                WHEN MAX(gogs.action.created) >= MAX(gogs.user.updated) THEN MAX(gogs.action.created)
		                                WHEN MAX(gogs.user.updated) >= MAX(gogs.action.created) THEN MAX(gogs.user.updated)
		                                ELSE MAX(gogs.user.updated)
	                                END AS LastUpdate
                                FROM gogs.user
                                LEFT JOIN gogs.action ON gogs.user.id = gogs.action.act_user_id
                                WHERE gogs.user.login_name = {0}";
                var results = mySQL.Query(sql, new object[] { email });

                if (results != null && results.Any())
                {
                    var result = results.First();
                    DateTime tmpLast = lastActive;
                    DateTime.TryParse(result["LastUpdate"].ToString(), out tmpLast);
                    if (lastActive < tmpLast)
                        lastActive = tmpLast;
                }
            }
            return lastActive;
        }

        public static void AddUserGit(Config config, string username, string password)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    string email = GetUserEmailAddress(config, username);
                    // Add gogs user
                    using (var client = new WebClient())
                    {
                        var obj = new { source_id = config.GitConfig.SourceId, username = username, email = email, login_name = email, password = password };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        Uri baseUri = new Uri(config.GitConfig.Host);
                        Uri finalUri = new Uri(baseUri, "api/v1/admin/users?token=" + config.GitConfig.AccessToken);
                        string result = client.UploadString(finalUri, "POST", json);
                    }
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
                    using (var client = new WebClient())
                    {
                        var obj = new {source_id = config.GitConfig.SourceId, email = email, login_name = email, password = password};
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        Uri baseUri = new Uri(config.GitConfig.Host);
                        Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + username + "?token=" + config.GitConfig.AccessToken);
                        string result = client.UploadString(finalUri, "PATCH", json);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit git account password.", ex);
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
                    MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database.Server, config.GitConfig.Database.Database, config.GitConfig.Database.Username, config.GitConfig.Database.Password, config.GitConfig.Database.Port);

                    // Get the user's UID
                    string email = GetUserEmailAddress(config, username);
                    string userSelect = @"SELECT id FROM gogs.user WHERE gogs.user.login_name = {0}";
                    var uid = mySQL.ScalarQuery(userSelect, new object[] { email });

                    // See if they have Two Factor already
                    string sqlSelect = @"SELECT id 
                                FROM gogs.two_factor
                                LEFT JOIN gogs.user ON gogs.user.id = gogs.gogs.two_factor.uid
                                WHERE gogs.user.login_name = {0}";
                    var result = mySQL.ScalarQuery(sqlSelect, new object[] { email });
                    
                    if (result != null)
                    {
                        // They have an entry!  Let's update it
                        string insert = @"UPDATE gogs.two_factor SET uid = {1}, secret = {2}, scratch_token = {3}, updated_unix = {4} WHERE gogs.two_factor.id = {0}";

                        mySQL.Execute(insert, new object[] { result, uid, finalSecret, token, unixTime });
                    }
                    else
                    {
                        // They need a new entry
                        string update = @"INSERT INTO gogs.two_factor SET (uid, secret, scratch_token, created_unix, updated_unix) VALUES ({0}, {1}, {2}, {3}, {4})";

                        mySQL.Execute(update, new object[] { uid, finalSecret, token, unixTime, 0 });
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
                    MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database.Server, config.GitConfig.Database.Database, config.GitConfig.Database.Username, config.GitConfig.Database.Password, config.GitConfig.Database.Port);

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

                    try
                    {
                        Uri baseUri = new Uri(config.GitConfig.Host);
                        Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + username + "?token=" + config.GitConfig.AccessToken);
                        WebRequest request = WebRequest.Create(finalUri);
                        request.Method = "DELETE";

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode != HttpStatusCode.NotFound && response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                        {
                            throw new Exception("Unable to delete git account.  Response Code: " + response.StatusCode);
                        }
                    }
                    catch (HttpException htex)
                    {
                        if (htex.GetHttpCode() != 404)
                            throw new Exception("Unable to delete git account.  Http Exception: " + htex.Message);
                    }
                    catch (Exception ex)
                    {
                        // This error signifies the user doesn't exist, so we can continue deleting
                        if (ex.Message != "The remote server returned an error: (404) Not Found.")
                        {
                            throw new Exception("Unable to delete git account.  Exception: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete git account.", ex);
            }
        }
        #endregion

        public static HttpCookie CreateAuthCookie(string username, bool remember, string domain, bool local)
        {
            DateTime curTime = DateTime.Now;
            DateTime expireTime = curTime.AddMonths(1);

            Config config = Config.Load();
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, 
                username, 
                curTime,
                expireTime,
                remember,
                username
            );

            string encTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie authcookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            authcookie.HttpOnly = true;
            authcookie.Secure = true;
            if (remember)
            {
                authcookie.Expires = expireTime;
            }

            // Set domain dependent on where it's being ran from
            if (local) // localhost
            {
                authcookie.Domain = null;
            }
            else if (config.DevEnvironment) // dev.example.com
            {
                authcookie.Domain = string.Format("dev.{0}", domain);
            }
            else // A production instance
            {
                authcookie.Domain = string.Format(".{0}", domain);
            }

            return authcookie;
        }

        public static HttpCookie CreateTrustedDeviceCookie(string username, string domain, bool local)
        {
            Config config = Config.Load();

            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            HttpCookie trustCookie = new HttpCookie(Constants.TRUSTEDDEVICECOOKIE + "_" + username);
            trustCookie.Value = token;
            trustCookie.HttpOnly = true;
            trustCookie.Secure = true;
            trustCookie.Expires = DateTime.Now.AddYears(1);

            // Set domain dependent on where it's being ran from
            if (local) // localhost
            {
                trustCookie.Domain = null;
            }
            else if (config.DevEnvironment) // dev.example.com
            {
                trustCookie.Domain = string.Format("dev.{0}", domain);
            }
            else // A production instance
            {
                trustCookie.Domain = string.Format(".{0}", domain);
            }

            return trustCookie;
        }
    }
}
