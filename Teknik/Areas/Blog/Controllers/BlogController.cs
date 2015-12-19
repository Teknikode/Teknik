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
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Title = "Teknik Blog - " + Config.Title;
                blog = db.Blogs.Find(Constants.SERVERBLOGID);
            }
            else
            {
                var blogs = db.Blogs.Include("User").Where(p => p.User.Username == username);
                if (blogs.Any())
                {
                    blog = blogs.First();
                    ViewBag.Title = blog.User.Username + "'s Blog - " + Config.Title;
                }
            }
            // find the post specified
            if (blog != null)
            {
                var foundPosts = db.Posts.Include("Blog").Include("Blog.User").Where(p =>   (p.Blog.BlogId == blog.BlogId) &&
                                                                                            (p.Published || p.Blog.User.Username == User.Identity.Name)
                                                                                         ).OrderByDescending(p => p.DatePosted);
                model = new BlogViewModel();
                model.BlogId = blog.BlogId;
                model.UserId = blog.UserId;
                model.User = blog.User;
                model.Title = blog.Title;
                model.Description = blog.Description;
                model.Posts = (foundPosts != null && foundPosts.Any()) ? foundPosts.ToList() : null;

                return View(model);
            }
            return View(model);
        }

        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult Post(string username, int id)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // find the post specified
            Post post = db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.Blog.User.Username == username && p.PostId == id) && 
                                                                                (p.Published || p.Blog.User.Username == User.Identity.Name)
                                                                            ).First();
            if (post != null)
            {
                PostViewModel model = new PostViewModel(post);

                ViewBag.Title = model.Title + " - " + username + "'s Blog - " + Config.Title;
                return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", model);
            }
            return View("~/Areas/Blog/Views/Blog/ViewPost.cshtml", null);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPosts(int blogID, int startPostID, int count)
        {
            var posts = db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.BlogId == blogID && p.PostId > startPostID) &&
                                                                                (p.Published || p.Blog.User.Username == User.Identity.Name)
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
            Post post = (User.IsInRole("Admin")) ? db.Posts.Find(postID) : db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.PostId == postID) && 
                                                                                                                                    (p.Published || p.Blog.User.Username == User.Identity.Name)).First();
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
            return Json(new { error = "No post found" });
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
    }
}
