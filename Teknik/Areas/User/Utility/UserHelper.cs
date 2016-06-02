using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Shortener.Models;
using Teknik.Areas.Users.Models;
using Teknik.Configuration;
using Teknik.Helpers;
using Teknik.Models;

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
                user.BlogSettings = db.BlogSettings.Find(user.UserId);
                user.UploadSettings = db.UploadSettings.Find(user.UserId);
            }

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

        public static void AddUser(TeknikEntities db, Config config, User user, string password)
        {
            try
            {
                // Add User
                user.HashedPassword = SHA384.Hash(user.Username, password);
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

        public static void EditUser(TeknikEntities db, Config config, User user, bool changePass, string password)
        {
            try
            {
                // Changing Password?
                if (changePass)
                {
                    // Update User password
                    user.HashedPassword = SHA384.Hash(user.Username, password);
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to edit user.", ex);
            }
        }

        public static void DeleteUser(TeknikEntities db, Config config, User user)
        {
            try
            {
                // Update uploads
                List<Upload.Models.Upload> uploads = db.Uploads.Include("User").Where(u => u.User.Username == user.Username).ToList();
                if (uploads != null)
                {
                    foreach (Upload.Models.Upload upload in uploads)
                    {
                        upload.UserId = null;
                        db.Entry(upload).State = EntityState.Modified;
                    }
                }

                // Update pastes
                List<Paste.Models.Paste> pastes = db.Pastes.Include("User").Where(u => u.User.Username == user.Username).ToList();
                if (pastes != null)
                {
                    foreach (Paste.Models.Paste paste in pastes)
                    {
                        paste.UserId = null;
                        db.Entry(paste).State = EntityState.Modified;
                    }
                }

                // Update shortened urls
                List<ShortenedUrl> shortUrls = db.ShortenedUrls.Include("User").Where(u => u.User.Username == user.Username).ToList();
                if (shortUrls != null)
                {
                    foreach (ShortenedUrl shortUrl in shortUrls)
                    {
                        shortUrl.UserId = null;
                        db.Entry(shortUrl).State = EntityState.Modified;
                    }
                }

                // Delete Blogs
                Blog.Models.Blog blog = db.Blogs.Include("BlogPosts").Include("BlogPosts.Comments").Include("User").Where(u => u.User.Username == user.Username).FirstOrDefault();
                if (blog != null)
                {
                    db.Blogs.Remove(blog);
                }

                // Delete post comments
                List<BlogPostComment> postComments = db.BlogComments.Include("User").Where(u => u.User.Username == user.Username).ToList();
                if (postComments != null)
                {
                    foreach (BlogPostComment postComment in postComments)
                    {
                        db.BlogComments.Remove(postComment);
                    }
                }

                // Delete podcast comments
                List<Podcast.Models.PodcastComment> podComments = db.PodcastComments.Include("User").Where(u => u.User.Username == user.Username).ToList();
                if (podComments != null)
                {
                    foreach (Podcast.Models.PodcastComment podComment in podComments)
                    {
                        db.PodcastComments.Remove(podComment);
                    }
                }

                // Delete User
                db.Users.Remove(user);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to delete user.", ex);
            }
        }

        public static void SendRecoveryEmailVerification(Config config, string username, string email, string resetUrl, string verifyUrl)
        {
            SmtpClient client = new SmtpClient();
            client.Host = config.ContactConfig.Host;
            client.Port = config.ContactConfig.Port;
            client.EnableSsl = config.ContactConfig.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.Credentials = new NetworkCredential(config.NoReplyEmail, config.ContactConfig.Password);
            client.Timeout = 5000;

            MailMessage mail = new MailMessage(config.NoReplyEmail, email);
            mail.Subject = "Recovery Email Validation";
            mail.Body = string.Format(@"Thank you {0} for signing up for Teknik!  

You are recieving this email because you have specified this email address as your recovery email.  In the event that you forget your password, you can visit {1} and request a temporary password reset key be sent to this email.  You will then be able to reset and choose a new password.

In order to verify that you own this email, please click the following link or paste it into your browser: {2}  

If you recieved this email and you did not sign up for an account, please email us at {3} and ignore the verification link.

Thank you and enjoy!

- Teknik Administration", username, resetUrl, verifyUrl, config.SupportEmail);
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
                string email = GetUserEmailAddress(config, username);
                // We need to check the actual git database
                MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database);
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
                    string email = GetUserEmailAddress(config, username);
                    using (var client = new WebClient())
                    {
                        var obj = new { source_id = config.GitConfig.SourceId, email = email, password = password };
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

        public static void DeleteUserGit(Config config, string username)
        {
            try
            {
                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    try
                    {
                        Uri baseUri = new Uri(config.GitConfig.Host);
                        Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + username + "?token=" + config.GitConfig.AccessToken);
                        WebRequest request = WebRequest.Create(finalUri);
                        request.Method = "DELETE";

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode != HttpStatusCode.NotFound && response.StatusCode != HttpStatusCode.OK)
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
            Config config = Config.Load();
            HttpCookie authcookie = FormsAuthentication.GetAuthCookie(username, remember);
            authcookie.Name = "TeknikAuth";
            authcookie.HttpOnly = true;
            authcookie.Secure = true;

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
    }
}
