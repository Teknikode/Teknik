using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
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

        public static void AddUser(TeknikEntities db, Config config, User user, string password)
        {
            try
            {
                string email = string.Format("{0}@{1}", user.Username, config.EmailConfig.Domain);
                // If Email Server is enabled
                if (config.EmailConfig.Enabled)
                {
                    // Connect to hmailserver COM
                    var app = new hMailServer.Application();
                    app.Connect();
                    app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);

                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    try
                    {
                        var account = domain.Accounts.ItemByAddress[email];
                        throw new Exception("That email already exists.");
                    }
                    catch { }

                    // If we got an exception, then the email doesnt exist and we continue on!
                    var newAccount = domain.Accounts.Add();
                    newAccount.Address = email;
                    newAccount.Password = password;
                    newAccount.Active = true;
                    newAccount.MaxSize = config.EmailConfig.MaxSize;

                    newAccount.Save();
                }

                // If Git is enabled
                if (config.GitConfig.Enabled)
                {
                    // Add gogs user
                    using (var client = new WebClient())
                    {
                        var obj = new { source_id = config.GitConfig.SourceId, username = user.Username, email = email, login_name = email, password = password };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        Uri baseUri = new Uri(config.GitConfig.Host);
                        Uri finalUri = new Uri(baseUri, "api/v1/admin/users?token=" + config.GitConfig.AccessToken);
                        string result = client.UploadString(finalUri, "POST", json);
                    }
                }

                // Add User
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

        public static void SaveUser(TeknikEntities db, Config config, User user, bool changePass, string newPass)
        {
            string email = string.Format("{0}@{1}", user.Username, config.EmailConfig.Domain);
            // Changing Password?
            if (changePass)
            {
                // Update Email Pass
                if (config.EmailConfig.Enabled)
                {
                    try
                    {
                        var app = new hMailServer.Application();
                        app.Connect();
                        app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);
                        var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                        var account = domain.Accounts.ItemByAddress[email];
                        account.Password = newPass;
                        account.Save();
                    }
                    catch (COMException)
                    { }
                }

                // Update Git Pass
                if (config.GitConfig.Enabled)
                {
                    using (var client = new WebClient())
                    {
                        var obj = new { source_id = config.GitConfig.SourceId, email = email, password = newPass };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        Uri baseUri = new Uri(config.GitConfig.Host);
                        Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + user.Username + "?token=" + config.GitConfig.AccessToken);
                        string result = client.UploadString(finalUri, "PATCH", json);
                    }
                }
            }
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void DeleteUser(TeknikEntities db, Config config, User user)
        {
            // Check to see if we need to delete their email.
            if (config.EmailConfig.Enabled)
            {
                try
                {
                    // Delete Email
                    var app = new hMailServer.Application();
                    app.Connect();
                    app.Authenticate(config.EmailConfig.Username, config.EmailConfig.Password);
                    var domain = app.Domains.ItemByName[config.EmailConfig.Domain];
                    var account = domain.Accounts.ItemByAddress[string.Format("{0}@{1}", user.Username, config.EmailConfig.Domain)];
                    if (account != null)
                    {
                        account.Delete();
                    }
                }
                catch (COMException)
                {
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to delete email account.", ex);
                }
            }

            // Delete Git
            if (config.GitConfig.Enabled)
            {
                try
                {
                    Uri baseUri = new Uri(config.GitConfig.Host);
                    Uri finalUri = new Uri(baseUri, "api/v1/admin/users/" + user.Username + "?token=" + config.GitConfig.AccessToken);
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

            // Delete post comments
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

        public static DateTime GetLastActivity(TeknikEntities db, Config config, User user)
        {
            DateTime lastActive = new DateTime(1900, 1, 1);
            string email = string.Format("{0}@{1}", user.Username, config.EmailConfig.Domain);

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
                    DateTime lastEmail = (DateTime)account.LastLogonTime;
                    if (lastActive < lastEmail)
                        lastActive = lastEmail;
                }
                catch (Exception ex) { }
            }
            
            if (config.GitConfig.Enabled)
            {
                // We need to check the actual git database
                MysqlDatabase mySQL = new MysqlDatabase(config.GitConfig.Database);
                string sql = @"SELECT MAX(gogs.repository.updated) AS LastUpdate 
                                FROM gogs.repository
                                INNER JOIN gogs.user
                                WHERE gogs.user.login_name = {0} AND gogs.user.id = gogs.repository.owner_id";
                object result = mySQL.ScalarQuery(sql, new object[] { email });

                if (result != null)
                {
                    DateTime tmpLast = lastActive;
                    DateTime.TryParse(result.ToString(), out tmpLast);
                    if (lastActive < tmpLast)
                        lastActive = tmpLast;
                }
            }

            if (lastActive < user.LastSeen)
                lastActive = user.LastSeen;

            return lastActive;
        }

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
