using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Error.Controllers;
using Teknik.Areas.Error.ViewModels;
using Teknik.Areas.Profile.Models;
using Teknik.Areas.Profile.ViewModels;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;
using Teknik.ViewModels;
using System.Windows;
using System.Net;

namespace Teknik.Areas.Profile.Controllers
{
    public class ProfileController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Profile/Profile
        [AllowAnonymous]
        public ActionResult Index(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = User.Identity.Name;
            }

            ProfileViewModel model = new ProfileViewModel();
            ViewBag.Title = "User Does Not Exist - " + Config.Title;
            ViewBag.Message = "The User does not exist";

            User user = db.Users.Where(u => u.Username == username).FirstOrDefault();

            if (user != null)
            {
                ViewBag.Title = username + "'s Profile - " + Config.Title;
                ViewBag.Message = "Viewing " + username + "'s Profile";
                
                model.UserID = user.UserId;
                model.Username = user.Username;
                model.Email = string.Format("{0}@{1}", user.Username, Config.EmailConfig.Domain);
                model.JoinDate = user.JoinDate;
                model.LastSeen = user.LastSeen;

                model.UserSettings = user.UserSettings;
                model.BlogSettings = user.BlogSettings;
                model.UploadSettings = user.UploadSettings;

                model.Uploads = db.Uploads.Where(u => u.UserId == user.UserId).OrderByDescending(u => u.DateUploaded).ToList();

                model.Pastes = db.Pastes.Where(u => u.UserId == user.UserId).OrderByDescending(u => u.DatePosted).ToList();

                return View(model);
            }
            model.Error = true;
            return View(model);
        }

        // GET: Profile/Profile
        [AllowAnonymous]
        public ActionResult Settings()
        {
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;

                SettingsViewModel model = new SettingsViewModel();
                ViewBag.Title = "User Does Not Exist - " + Config.Title;
                ViewBag.Message = "The User does not exist";

                User user = db.Users.Where(u => u.Username == username).FirstOrDefault();

                if (user != null)
                {
                    ViewBag.Title = "Settings - " + Config.Title;
                    ViewBag.Message = "Your " + Config.Title + " Settings";

                    model.UserID = user.UserId;
                    model.Username = user.Username;

                    model.UserSettings = user.UserSettings;
                    model.BlogSettings = user.BlogSettings;
                    model.UploadSettings = user.UploadSettings;

                    return View(model);
                }
                model.Error = true;
                return View(model);
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http403"));
        }

        [HttpGet]
        [AllowAnonymous]
        // GET: Profile
        public ActionResult Login(string ReturnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/Profile/Views/Profile/ViewLogin.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = model.Username;
                string password = SHA384.Hash(model.Username, model.Password);
                bool userValid = db.Users.Any(b => b.Username == username && b.HashedPassword == password);
                if (userValid)
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Json(new { result = "true" });
                    }
                    else
                    {
                        return Redirect(model.ReturnUrl);
                    }
                }
            }
            return Json(new { error = "Invalid User name or Password." });
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect(Url.SubRouteUrl("www", "Home.Index"));
        }

        [HttpGet]
        [AllowAnonymous]
        // GET: Profile
        public ActionResult Register(string ReturnUrl)
        {
            RegisterViewModel model = new RegisterViewModel();
            model.ReturnUrl = ReturnUrl;

            return View("/Areas/Profile/Views/Profile/ViewRegistration.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Config.UserConfig.RegistrationEnabled)
                {
                    var foundUser = db.Users.Where(b => b.Username == model.Username).FirstOrDefault();
                    if (foundUser != null)
                    {
                        return Json(new { error = "That username already exists." });
                    }
                    if (model.Password != model.ConfirmPassword)
                    {
                        return Json(new { error = "Passwords must match." });
                    }
                    try
                    {
                        string email = string.Format("{0}@{1}", model.Username, Config.EmailConfig.Domain);
                        // If Email Server is enabled
                        if (Config.EmailConfig.Enabled)
                        {
                            // Connect to hmailserver COM
                            var app = new hMailServer.Application();
                            app.Connect();
                            app.Authenticate(Config.EmailConfig.Username, Config.EmailConfig.Password);

                            var domain = app.Domains.ItemByName[Config.EmailConfig.Domain];
                            try
                            {
                                var account = domain.Accounts.ItemByAddress[email];
                                return Json(new { error = "That email already exists." });
                            }
                            catch { }

                            // If we got an exception, then the email doesnt exist and we continue on!
                            var newAccount = domain.Accounts.Add();
                            newAccount.Address = email;
                            newAccount.Password = model.Password;
                            newAccount.Active = true;
                            newAccount.MaxSize = Config.EmailConfig.MaxSize;

                            newAccount.Save();
                        }

                        // If Git is enabled
                        if (Config.GitConfig.Enabled)
                        {
                            // Add gogs user
                            using (var client = new WebClient())
                            {
                                var obj = new { username = model.Username, email = email, password = model.Password };
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                                Uri baseUri = new Uri(Config.GitConfig.Host);
                                Uri finalUri = new Uri(baseUri, "admin/users?token=" + Config.GitConfig.AccessToken);
                                string result = client.UploadString(finalUri, "POST", json);
                            }
                        }

                        // Add User
                        User newUser = db.Users.Create();
                        newUser.JoinDate = DateTime.Now;
                        newUser.Username = model.Username;
                        newUser.HashedPassword = SHA384.Hash(model.Username, model.Password);
                        newUser.UserSettings = new UserSettings();
                        newUser.BlogSettings = new BlogSettings();
                        newUser.UploadSettings = new UploadSettings();
                        db.Users.Add(newUser);
                        db.SaveChanges();

                        // Generate blog for the user
                        var newBlog = db.Blogs.Create();
                        newBlog.UserId = db.Users.Where(u => u.Username == model.Username).Select(u => u.UserId).First();
                        db.Blogs.Add(newBlog);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return Json(new { error = "Unable to create the user." });
                    }
                    return Login(new LoginViewModel { Username = model.Username, Password = model.Password, RememberMe = false, ReturnUrl = model.ReturnUrl });
                }
                return Json(new { error = "User Registration is Disabled" });
            }
            return Json(new { error = "You must include all fields." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string curPass, string newPass, string newPassConfirm, string website, string quote, string about, string blogTitle, string blogDesc, bool saveKey, bool serverSideEncrypt)
        {
            if (ModelState.IsValid)
            {
                User user = db.Users.Where(u => u.Username == User.Identity.Name).First();
                if (user != null)
                {
                    string email = string.Format("{0}@{1}", User.Identity.Name, Config.EmailConfig.Domain);
                    // Changing Password?
                    if (!string.IsNullOrEmpty(curPass) && (!string.IsNullOrEmpty(newPass) || !string.IsNullOrEmpty(newPassConfirm)))
                    {
                        // Old Password Valid?
                        if (SHA384.Hash(User.Identity.Name, curPass) != user.HashedPassword)
                        {
                            return Json(new { error = "Invalid Original Password." });
                        }
                        // The New Password Match?
                        if (newPass != newPassConfirm)
                        {
                            return Json(new { error = "New Password Must Match." });
                        }
                        user.HashedPassword = SHA384.Hash(User.Identity.Name, newPass);

                        // Update Email Pass
                        if (Config.EmailConfig.Enabled)
                        {
                            var app = new hMailServer.Application();
                            app.Connect();
                            app.Authenticate(Config.EmailConfig.Username, Config.EmailConfig.Password);
                            var domain = app.Domains.ItemByName[Config.EmailConfig.Domain];
                            var account = domain.Accounts.ItemByAddress[email];
                            account.Password = newPass;
                            account.Save();
                        }                        

                        // Update Git Pass
                        if (Config.GitConfig.Enabled)
                        {
                            using (var client = new WebClient())
                            {
                                var obj = new { source_id = 1, email = email, password = newPass };
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                                Uri baseUri = new Uri(Config.GitConfig.Host);
                                Uri finalUri = new Uri(baseUri, "admin/users/" + User.Identity.Name + "?token=" + Config.GitConfig.AccessToken);
                                string result = client.UploadString(finalUri, "PATCH", json);
                            }
                        }
                    }

                    user.UserSettings.Website = website;
                    user.UserSettings.Quote = quote;
                    user.UserSettings.About = about;

                    user.BlogSettings.Title = blogTitle;
                    user.BlogSettings.Description = blogDesc;

                    user.UploadSettings.SaveKey = saveKey;
                    user.UploadSettings.ServerSideEncrypt = serverSideEncrypt;

                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = true });
                }
                return Json(new { error = "User does not exist" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete()
        {
            if (ModelState.IsValid)
            {
                // Delete Email
                var app = new hMailServer.Application();
                app.Connect();
                app.Authenticate(Config.EmailConfig.Username, Config.EmailConfig.Password);
                var domain = app.Domains.ItemByName[Config.EmailConfig.Domain];
                var account = domain.Accounts.ItemByAddress[string.Format("{0}@{1}", User.Identity.Name, Config.EmailConfig.Domain)];
                account.Delete();

                // Delete Git
                if (Config.GitConfig.Enabled)
                {
                    Uri baseUri = new Uri(Config.GitConfig.Host);
                    Uri finalUri = new Uri(baseUri, "admin/users/" + User.Identity.Name + "?token=" + Config.GitConfig.AccessToken);
                    WebRequest request = WebRequest.Create(finalUri);
                    request.Method = "DELETE";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                }

                // Update uploads
                List<Upload.Models.Upload> uploads = db.Uploads.Include("User").Where(u => u.User.Username == User.Identity.Name).ToList();
                if (uploads != null)
                {
                    foreach (Upload.Models.Upload upload in uploads)
                    {
                        upload.UserId = null;
                        db.Entry(upload).State = EntityState.Modified;
                    }
                }

                // Update pastes
                List<Paste.Models.Paste> pastes = db.Pastes.Include("User").Where(u => u.User.Username == User.Identity.Name).ToList();
                if (pastes != null)
                {
                    foreach (Paste.Models.Paste paste in pastes)
                    {
                        paste.UserId = null;
                        db.Entry(paste).State = EntityState.Modified;
                    }
                }

                // Delete Blogs
                Blog.Models.Blog blog = db.Blogs.Include("BlogPosts").Include("BlogPosts.Comments").Include("User").Where(u => u.User.Username == User.Identity.Name).FirstOrDefault();
                if (blog != null)
                {
                    db.Blogs.Remove(blog);
                }

                // Delete post comments
                List<BlogPostComment> postComments = db.BlogComments.Include("User").Where(u => u.User.Username == User.Identity.Name).ToList();
                if (postComments != null)
                {
                    foreach (BlogPostComment postComment in postComments)
                    {
                        db.BlogComments.Remove(postComment);
                    }
                }

                // Delete post comments
                List<Podcast.Models.PodcastComment> podComments = db.PodcastComments.Include("User").Where(u => u.User.Username == User.Identity.Name).ToList();
                if (podComments != null)
                {
                    foreach (Podcast.Models.PodcastComment podComment in podComments)
                    {
                        db.PodcastComments.Remove(podComment);
                    }
                }

                // Delete User
                User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                if (user != null)
                {
                    user.UserSettings = db.UserSettings.Find(user.UserId);
                    user.BlogSettings = db.BlogSettings.Find(user.UserId);
                    user.UploadSettings = db.UploadSettings.Find(user.UserId);
                    db.Users.Remove(user);
                    db.SaveChanges();
                    FormsAuthentication.SignOut();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "Unable to delete user" });
        }
    }
}