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
using Teknik.Areas.Profile.Models;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.Blog.Controllers
{
    public class BlogController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult Blog(string username)
        {
            Models.Blog blog = null;
            BlogViewModel model = null;
            // The blog is the main site's blog
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Title = "Teknik Blog - " + Config.Title;
                var blogs = db.Blogs.Include("User").Where(p => (p.BlogId == Constants.SERVERBLOGID));
                if (blogs != null && blogs.Any())
                {
                    blog = blogs.First();
                    blog.Title = Config.BlogConfig.Title;
                    blog.Description = Config.BlogConfig.Description;
                }
            }
            else // A user specific blog
            {
                var blogs = db.Blogs.Include("User").Where(p => p.User.Username == username && p.BlogId != Constants.SERVERBLOGID);
                if (blogs.Any())
                {
                    blog = blogs.First();
                    ViewBag.Title = blog.User.Username + "'s Blog - " + Config.Title;
                }
            }
            // find the blog specified
            if (blog != null)
            {
                var foundPosts = (User.IsInRole("Admin"))   ? db.Posts.Include("Blog").Include("Blog.User").Where(p =>  (p.BlogId == blog.BlogId))
                                                            : db.Posts.Include("Blog").Include("Blog.User").Where(p =>  (p.BlogId == blog.BlogId) &&
                                                                                                                        (p.Published || p.Blog.User.Username == User.Identity.Name));
                model = new BlogViewModel();
                model.BlogId = blog.BlogId;
                model.UserId = blog.UserId;
                model.User = blog.User;
                model.Title = blog.Title;
                model.Description = blog.Description;
                model.HasPosts = (foundPosts != null && foundPosts.Any());

                return View(model);
            }
            return View(model);
        }

        #region Posts
        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult Post(string username, int id)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // find the post specified
            var posts = (User.IsInRole("Admin"))    ? db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.Blog.User.Username == username && p.PostId == id))
                                                    : db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.Blog.User.Username == username && p.PostId == id) && 
                                                                                (p.Published || p.Blog.User.Username == User.Identity.Name));
            if (posts != null && posts.Any())
            {
                PostViewModel model = new PostViewModel(posts.First());

                ViewBag.Title = model.Title + " - " + username + "'s Blog - " + Config.Title;
                return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", model);
            }
            return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", null);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPosts(int blogID, int startPostID, int count)
        {
            var posts = (User.IsInRole("Admin"))    ? db.Posts.Include("Blog").Include("Blog.User").Where(p => p.BlogId == blogID).OrderByDescending(p => p.DatePosted).Skip(startPostID).Take(count).ToList()
                                                    : db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.BlogId == blogID) && (p.Published || p.Blog.User.Username == User.Identity.Name)
                                                                                                        ).OrderByDescending(p => p.DatePosted).Skip(startPostID).Take(count).ToList();
            List<PostViewModel> postViews = new List<PostViewModel>();
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    postViews.Add(new PostViewModel(post));
                }
            }
            return PartialView("~/Areas/Blog/Views/Blog/Posts.cshtml", postViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPostTitle(int postID)
        {
            string title = string.Empty;
            Post post = (User.IsInRole("Admin"))    ? db.Posts.Find(postID) 
                                                    : db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.PostId == postID) && (p.Published || p.Blog.User.Username == User.Identity.Name)).First();
            if (post != null)
            {
                return Json(new { result = post.Title });
            }
            return Json(new { error = "No title found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPostArticle(int postID)
        {
            string title = string.Empty;
            Post post = (User.IsInRole("Admin")) ? db.Posts.Find(postID) : db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.PostId == postID) && 
                                                                                                                                    (p.Published || p.Blog.User.Username == User.Identity.Name)).First();
            if (post != null)
            {
                return Json(new { result = post.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePost(int blogID, string title, string article)
        {
            if (ModelState.IsValid)
            {
                Post post = db.Posts.Create();
                post.BlogId = blogID;
                post.Title = title;
                post.Article = article;
                post.DatePosted = DateTime.Now;
                post.DatePublished = DateTime.Now;

                db.Posts.Add(post);
                db.SaveChanges();
                return Json(new { result = true });
            }
            return Json(new { error = "No post created" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int postID, string title, string article)
        {
            if (ModelState.IsValid)
            {
                Post post = db.Posts.Find(postID);
                if (post != null)
                {
                    post.Title = title;
                    post.Article = article;
                    db.Entry(post).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No post found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PublishPost(int postID, bool publish)
        {
            if (ModelState.IsValid)
            {
                Post post = db.Posts.Find(postID);
                if (post != null)
                {
                    post.Published = publish;
                    if (publish)
                        post.DatePublished = DateTime.Now;
                    db.Entry(post).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No post found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int postID)
        {
            if (ModelState.IsValid)
            {
                Post post = db.Posts.Find(postID);
                if (post != null)
                {
                    db.Posts.Remove(post);
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No post found" });
        }
        #endregion

        #region Comments
        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetComments(int postID, int startCommentID, int count)
        {
            var comments = db.BlogComments.Include("Post").Include("Post.Blog").Include("Post.Blog.User").Where(p => (p.PostId == postID)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
            List<CommentViewModel> commentViews = new List<CommentViewModel>();
            if (comments != null)
            {
                foreach (Comment comment in comments)
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
            Comment comment = db.BlogComments.Include("Post").Include("Post.Blog").Include("Post.Blog.User").Where(p => (p.CommentId == commentID)).First();
            if (comment != null)
            {
                return Json(new { result = comment.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateComment(int postID, string article)
        {
            if (ModelState.IsValid)
            {
                Comment comment = db.BlogComments.Create();
                comment.PostId = postID;
                comment.UserId = db.Users.Where(u => u.Username == User.Identity.Name).First().UserId;
                comment.Article = article;
                comment.DatePosted = DateTime.Now;

                db.BlogComments.Add(comment);
                db.SaveChanges();
                return Json(new { result = true });
            }
            return Json(new { error = "No comment created" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditComment(int commentID, string article)
        {
            if (ModelState.IsValid)
            {
                Comment comment = db.BlogComments.Find(commentID);
                if (comment != null)
                {
                    comment.Article = article;
                    db.Entry(comment).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No comment found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int commentID)
        {
            if (ModelState.IsValid)
            {
                Comment comment = db.BlogComments.Find(commentID);
                if (comment != null)
                {
                    db.BlogComments.Remove(comment);
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No comment found" });
        }
        #endregion
    }
}
