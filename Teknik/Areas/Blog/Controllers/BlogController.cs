using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Blog.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.Blog.Controllers
{
    public class BlogController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Blogs/Details/5
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Blog(string username)
        {
            BlogViewModel model = new BlogViewModel();
            // The blog is the main site's blog
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Title = Config.BlogConfig.Title + " - " + Config.Title;
                ViewBag.Description = Config.BlogConfig.Description;
                bool isAuth = User.IsInRole("Admin");
                var foundPosts = db.BlogPosts.Where(p => ((p.System || isAuth) && p.Published));
                model = new BlogViewModel();
                model.BlogId = Config.BlogConfig.ServerBlogId;

                User user = (User.IsInRole("Admin")) ? UserHelper.GetUser(db, User.Identity.Name) : null;
                model.UserId = (user != null) ? user.UserId : 0;
                model.User = user;
                model.Title = Config.BlogConfig.Title;
                model.Description = Config.BlogConfig.Description;
                model.HasPosts = (foundPosts != null && foundPosts.Any());

                return View(model);
            }
            else // A user specific blog
            {
                Models.Blog blog = db.Blogs.Where(p => p.User.Username == username && p.BlogId != Config.BlogConfig.ServerBlogId).FirstOrDefault();
                // find the blog specified
                if (blog != null)
                {
                    ViewBag.Title = blog.User.Username + "'s Blog - " + Config.Title;
                    if (!string.IsNullOrEmpty(blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Description = blog.User.BlogSettings.Description;
                    bool isAuth = User.IsInRole("Admin");
                    var foundPosts = db.BlogPosts.Where(p => (p.BlogId == blog.BlogId && !p.System) && 
                                                                                                    (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth)).FirstOrDefault();
                    model = new BlogViewModel();
                    model.BlogId = blog.BlogId;
                    model.UserId = blog.UserId;
                    model.User = blog.User;
                    model.Title = blog.User.BlogSettings.Title;
                    model.Description = blog.User.BlogSettings.Description;
                    model.HasPosts = (foundPosts != null);

                    return View(model);
                }
            }
            model.Error = true;
            return View(model);
        }

        #region Posts
        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Post(string username, int id)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostViewModel model = new PostViewModel();
            // find the post specified
            bool isAuth = User.IsInRole("Admin");
            var post = db.BlogPosts.Where(p => p.BlogPostId == id && (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth)).FirstOrDefault();
            if (post != null)
            {
                model = new PostViewModel(post);

                if (post.System)
                {
                    ViewBag.Title = model.Title + " - " + Config.BlogConfig.Title + " - " + Config.Title;
                    ViewBag.Description = Config.BlogConfig.Description;
                }
                else
                {
                    ViewBag.Title = username + "'s Blog - " + Config.Title;
                    if (!string.IsNullOrEmpty(post.Blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = post.Blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Title = model.Title + " - " + ViewBag.Title;
                    ViewBag.Description = post.Blog.User.BlogSettings.Description;
                }
                return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "Blog Post does not exist.";
            return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", model);
        }
        
        public ActionResult NewPost(string username, int blogID)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogViewModel model = new BlogViewModel();
            // find the post specified
            bool isAuth = User.IsInRole("Admin");
            var blog = db.Blogs.Where(p => (p.BlogId == blogID) && (p.User.Username == User.Identity.Name || isAuth)).FirstOrDefault();
            if (blog != null)
            {
                model = new BlogViewModel(blog);
                if (blog.User.Username == Constants.SERVERUSER)
                {
                    ViewBag.Title = model.Title + " - " + Config.BlogConfig.Title + " - " + Config.Title;
                    ViewBag.Description = Config.BlogConfig.Description;
                }
                else
                {
                    ViewBag.Title = username + "'s Blog - " + Config.Title;
                    if (!string.IsNullOrEmpty(blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Title = model.Title + " - " + ViewBag.Title;
                    ViewBag.Description = blog.User.BlogSettings.Description;
                }
                return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "Blog does not exist.";
            return View("~/Areas/Blog/Views/Blog/Blog.cshtml", model);
        }
        public ActionResult EditPost(string username, int id)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PostViewModel model = new PostViewModel();
            // find the post specified
            bool isAuth = User.IsInRole("Admin");
            var post = db.BlogPosts.Where(p => (p.Blog.User.Username == username && p.BlogPostId == id) &&
                                                                                        (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth)).FirstOrDefault();
            if (post != null)
            {
                model = new PostViewModel(post);

                if (post.System)
                {
                    ViewBag.Title = model.Title + " - " + Config.BlogConfig.Title + " - " + Config.Title;
                    ViewBag.Description = Config.BlogConfig.Description;
                }
                else
                {
                    ViewBag.Title = username + "'s Blog - " + Config.Title;
                    if (!string.IsNullOrEmpty(post.Blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = post.Blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Title = model.Title + " - " + ViewBag.Title;
                    ViewBag.Description = post.Blog.User.BlogSettings.Description;
                }
                return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "Blog Post does not exist.";
            return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPosts(int blogID, int startPostID, int count)
        {
            bool isAuth = User.IsInRole("Admin");
            var posts = db.BlogPosts.Where(p => ((p.BlogId == blogID && !p.System) || (p.System && blogID == Config.BlogConfig.ServerBlogId)) && 
                                                                                        (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth)).OrderByDescending(p => p.DatePosted).Skip(startPostID).Take(count).ToList();
            List<PostViewModel> postViews = new List<PostViewModel>();
            if (posts != null)
            {
                foreach (BlogPost post in posts)
                {
                    postViews.Add(new PostViewModel(post));
                }
            }
            return PartialView("~/Areas/Blog/Views/Blog/Posts.cshtml", postViews);
        }

        [HttpPost]
        public ActionResult CreatePost(int blogID, string title, string article)
        {
            BlogViewModel model = new BlogViewModel();
            if (ModelState.IsValid)
            {
                bool isAuth = User.IsInRole("Admin");
                var blog = db.Blogs.Where(p => (p.BlogId == blogID) && (p.User.Username == User.Identity.Name || isAuth)).FirstOrDefault();
                if (blog != null)
                {
                    if (User.IsInRole("Admin") || db.Blogs.Where(b => b.User.Username == User.Identity.Name).FirstOrDefault() != null)
                    {
                        // Validate the fields
                        if (string.IsNullOrEmpty(title))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the title";
                            return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
                        }

                        if (string.IsNullOrEmpty(article))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the article";
                            return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
                        }

                        bool system = (blogID == Config.BlogConfig.ServerBlogId);
                        if (system)
                        {
                            var user = db.Blogs.Where(b => b.User.Username == User.Identity.Name);
                            if (user != null)
                            {
                                blogID = user.First().BlogId;
                            }
                        }
                        BlogPost post = db.BlogPosts.Create();
                        post.BlogId = blogID;
                        post.Title = title;
                        post.Article = article;
                        post.System = system;
                        post.DatePosted = DateTime.Now;
                        post.DatePublished = DateTime.Now;
                        post.DateEdited = DateTime.Now;

                        db.BlogPosts.Add(post);
                        db.SaveChanges();
                        return Redirect(Url.SubRouteUrl("blog", "Blog.Post", new { username = blog.User.Username, id = post.BlogPostId }));
                    }
                    model.Error = true;
                    model.ErrorMessage = "You are not authorized to create a post for this blog";
                    return View("~/Areas/Blog/Views/Blog/Blog.cshtml", model);
                }
                model.Error = true;
                model.ErrorMessage = "Blog does not exist.";
                return View("~/Areas/Blog/Views/Blog/Blog.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "No post created";
            return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
        }

        [HttpPost]
        public ActionResult EditPost(int postID, string title, string article)
        {
            PostViewModel model = new PostViewModel();
            if (ModelState.IsValid)
            {
                BlogPost post = db.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault();
                if (post != null)
                {
                    model = new PostViewModel(post);
                    if (User.IsInRole("Admin") || post.Blog.User.Username == User.Identity.Name)
                    {
                        // Validate the fields
                        if (string.IsNullOrEmpty(title))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the title";
                            return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
                        }

                        if (string.IsNullOrEmpty(article))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the article";
                            return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
                        }

                        post.Title = title;
                        post.Article = article;
                        post.DateEdited = DateTime.Now;
                        db.Entry(post).State = EntityState.Modified;
                        db.SaveChanges();
                        return Redirect(Url.SubRouteUrl("blog", "Blog.Post", new { username = post.Blog.User.Username, id = post.BlogPostId }));
                    }
                    model.Error = true;
                    model.ErrorMessage = "You are not authorized to edit this post";
                    return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
                }
                model.Error = true;
                model.ErrorMessage = "Post does not exist.";
                return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "Invalid Parameters";
            return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
        }

        [HttpPost]
        public ActionResult PublishPost(int postID, bool publish)
        {
            if (ModelState.IsValid)
            {
                BlogPost post = db.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault();
                if (post != null)
                {
                    if (User.IsInRole("Admin") || post.Blog.User.Username == User.Identity.Name)
                    {
                        post.Published = publish;
                        if (publish)
                            post.DatePublished = DateTime.Now;
                        db.Entry(post).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You are not authorized to publish this post" });
                }
                return Json(new { error = "No post found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public ActionResult DeletePost(int postID)
        {
            if (ModelState.IsValid)
            {
                BlogPost post = db.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault();
                if (post != null)
                {
                    if (User.IsInRole("Admin") || post.Blog.User.Username == User.Identity.Name)
                    {
                        db.BlogPosts.Remove(post);
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You are not authorized to delete this post" });
                }
                return Json(new { error = "No post found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }
        #endregion

        #region Comments
        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetComments(int postID, int startCommentID, int count)
        {
            var comments = db.BlogComments.Where(p => (p.BlogPostId == postID)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
            List<CommentViewModel> commentViews = new List<CommentViewModel>();
            if (comments != null)
            {
                foreach (BlogPostComment comment in comments)
                {
                    commentViews.Add(new CommentViewModel(comment));
                }
            }
            return PartialView("~/Areas/Blog/Views/Blog/Comments.cshtml", commentViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetCommentArticle(int commentID)
        {
            BlogPostComment comment = db.BlogComments.Where(p => (p.BlogPostCommentId == commentID)).First();
            if (comment != null)
            {
                return Json(new { result = comment.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        public ActionResult CreateComment(int postID, string article)
        {
            if (ModelState.IsValid)
            {
                if (db.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault() != null)
                {
                    BlogPostComment comment = db.BlogComments.Create();
                    comment.BlogPostId = postID;
                    comment.UserId = UserHelper.GetUser(db, User.Identity.Name).UserId;
                    comment.Article = article;
                    comment.DatePosted = DateTime.Now;
                    comment.DateEdited = DateTime.Now;

                    db.BlogComments.Add(comment);
                    db.SaveChanges();
                    return Json(new { result = true });
                }
                return Json(new { error = "The post does not exist" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public ActionResult EditComment(int commentID, string article)
        {
            if (ModelState.IsValid)
            {
                BlogPostComment comment = db.BlogComments.Where(c => c.BlogPostCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        comment.Article = article;
                        comment.DateEdited = DateTime.Now;
                        db.Entry(comment).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You don't have permission to edit this comment" });
                }
                return Json(new { error = "No comment found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public ActionResult DeleteComment(int commentID)
        {
            if (ModelState.IsValid)
            {
                BlogPostComment comment = db.BlogComments.Where(c => c.BlogPostCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        db.BlogComments.Remove(comment);
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You don't have permission to delete this comment" });
                }
                return Json(new { error = "No comment found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }
        #endregion
    }
}
