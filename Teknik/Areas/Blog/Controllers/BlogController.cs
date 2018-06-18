using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Blog.ViewModels;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Attributes;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teknik.Logging;

namespace Teknik.Areas.Blog.Controllers
{
    [TeknikAuthorize]
    [Area("Blog")]
    public class BlogController : DefaultController
    {
        public BlogController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        public IActionResult Blog(string username)
        {
            BlogViewModel model = new BlogViewModel();
            // The blog is the main site's blog
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Title = _config.BlogConfig.Title + " - " + _config.Title;
                ViewBag.Description = _config.BlogConfig.Description;
                bool isAuth = User.IsInRole("Admin");
                model = new BlogViewModel();
                model.BlogId = _config.BlogConfig.ServerBlogId;

                User user = (User.IsInRole("Admin")) ? UserHelper.GetUser(_dbContext, User.Identity.Name) : null;
                model.UserId = (user != null) ? user.UserId : 0;
                model.User = user;
                model.Title = _config.BlogConfig.Title;
                model.Description = _config.BlogConfig.Description;
                var posts = _dbContext.BlogPosts
                                            .Include(p => p.Blog)
                                            .Include(p => p.Blog.User)
                                            .Include(p => p.Comments)
                                            .Include(p => p.Tags)
                                            .Where(p => (p.System || isAuth) && p.Published).OrderByDescending(p => p.DatePosted)
                                            .OrderByDescending(p => p.DatePosted)
                                            .Take(_config.BlogConfig.PostsToLoad).ToList();

                List<PostViewModel> postViews = new List<PostViewModel>();
                if (posts != null)
                {
                    foreach (BlogPost post in posts)
                    {
                        postViews.Add(new PostViewModel(post));
                    }
                }
                model.Posts = postViews;

                return View(model);
            }
            else // A user specific blog
            {
                Models.Blog blog = _dbContext.Blogs
                                                .Include(b => b.User)
                                                .Include(b => b.User.BlogSettings)
                                                .Where(p => p.User.Username == username && p.BlogId != _config.BlogConfig.ServerBlogId)
                                                .FirstOrDefault();
                // find the blog specified
                if (blog != null)
                {
                    ViewBag.Title = blog.User.Username + "'s Blog - " + _config.Title;
                    if (!string.IsNullOrEmpty(blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Description = blog.User.BlogSettings.Description;
                    bool isAuth = User.IsInRole("Admin");
                    model = new BlogViewModel();
                    model.BlogId = blog.BlogId;
                    model.UserId = blog.UserId;
                    model.User = blog.User;
                    model.Title = blog.User.BlogSettings.Title;
                    model.Description = blog.User.BlogSettings.Description;
                    var posts = _dbContext.BlogPosts
                                    .Include(p => p.Blog)
                                    .Include(p => p.Blog.User)
                                    .Include(p => p.Comments)
                                    .Include(p => p.Tags)
                                    .Where(p => (p.BlogId == blog.BlogId && !p.System) && (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth))
                                    .OrderByDescending(p => p.DatePosted)
                                    .Take(_config.BlogConfig.PostsToLoad).ToList();
                    
                    List<PostViewModel> postViews = new List<PostViewModel>();
                    if (posts != null)
                    {
                        foreach (BlogPost post in posts)
                        {
                            postViews.Add(new PostViewModel(post));
                        }
                    }
                    model.Posts = postViews;

                    return View(model);
                }
            }
            model.Error = true;
            return View(model);
        }

        #region Posts
        [AllowAnonymous]
        public IActionResult Post(string username, int id)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            PostViewModel model = new PostViewModel();
            // find the post specified
            bool isAuth = User.IsInRole("Admin");
            var post = _dbContext.BlogPosts
                                    .Include(p => p.Blog)
                                    .Include(p => p.Blog.User)
                                    .Include(p => p.Comments)
                                    .Include(p => p.Tags)
                                    .Where(p => p.BlogPostId == id && (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth))
                                    .FirstOrDefault();
            if (post != null)
            {
                model = new PostViewModel(post);

                if (post.System)
                {
                    ViewBag.Title = model.Title + " - " + _config.BlogConfig.Title + " - " + _config.Title;
                    ViewBag.Description = _config.BlogConfig.Description;
                }
                else
                {
                    ViewBag.Title = username + "'s Blog - " + _config.Title;
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
        
        public IActionResult NewPost(string username, int blogID)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            BlogViewModel model = new BlogViewModel();
            // find the post specified
            bool isAuth = User.IsInRole("Admin");
            var blog = _dbContext.Blogs
                                    .Include(b => b.User)
                                    .Include(b => b.User.BlogSettings)
                                    .Where(p => (p.BlogId == blogID) && (p.User.Username == User.Identity.Name || isAuth))
                                    .FirstOrDefault();
            if (blog != null)
            {
                model = new BlogViewModel(blog);
                if (blog.User.Username == Constants.SERVERUSER)
                {
                    ViewBag.Title = "Create Post - " + _config.BlogConfig.Title + " - " + _config.Title;
                    ViewBag.Description = _config.BlogConfig.Description;
                }
                else
                {
                    ViewBag.Title = username + "'s Blog - " + _config.Title;
                    if (!string.IsNullOrEmpty(blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Title = "Create Post - " + ViewBag.Title;
                    ViewBag.Description = blog.User.BlogSettings.Description;
                }
                return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "Blog does not exist.";
            return View("~/Areas/Blog/Views/Blog/Blog.cshtml", model);
        }

        public IActionResult EditPost(string username, int id)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }
            PostViewModel model = new PostViewModel();
            // find the post specified
            bool isAuth = User.IsInRole("Admin");
            var post = _dbContext.BlogPosts
                                    .Include(p => p.Blog)
                                    .Include(p => p.Blog.User)
                                    .Include(p => p.Comments)
                                    .Include(p => p.Tags)
                                    .Where(p => (p.Blog.User.Username == username && p.BlogPostId == id) &&
                                                (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth))
                                    .FirstOrDefault();
            if (post != null)
            {
                model = new PostViewModel(post);

                if (post.System)
                {
                    ViewBag.Title = "Edit Post - " + model.Title + " - " + _config.BlogConfig.Title + " - " + _config.Title;
                    ViewBag.Description = _config.BlogConfig.Description;
                }
                else
                {
                    ViewBag.Title = username + "'s Blog - " + _config.Title;
                    if (!string.IsNullOrEmpty(post.Blog.User.BlogSettings.Title))
                    {
                        ViewBag.Title = post.Blog.User.BlogSettings.Title + " - " + ViewBag.Title;
                    }
                    ViewBag.Title = "Edit Post - " + model.Title + " - " + ViewBag.Title;
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
        public IActionResult GetPosts(int blogID, int startPostID, int count)
        {
            bool isAuth = User.IsInRole("Admin");
            var posts = _dbContext.BlogPosts
                                    .Include(p => p.Blog)
                                    .Include(p => p.Blog.User)
                                    .Include(p => p.Comments)
                                    .Include(p => p.Tags)
                                    .Where(p => ((p.BlogId == blogID && !p.System) || (p.System && blogID == _config.BlogConfig.ServerBlogId)) && 
                                                (p.Published || p.Blog.User.Username == User.Identity.Name || isAuth))
                                    .OrderByDescending(p => p.DatePosted)
                                    .Skip(startPostID)
                                    .Take(count)
                                    .ToList();
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
        public IActionResult CreatePost(CreatePostViewModel data)
        {
            BlogViewModel model = new BlogViewModel();
            if (ModelState.IsValid)
            {
                bool isAuth = User.IsInRole("Admin");
                var blog = _dbContext.Blogs.Where(p => (p.BlogId == data.BlogId) && (p.User.Username == User.Identity.Name || isAuth)).FirstOrDefault();
                if (blog != null)
                {
                    if (User.IsInRole("Admin") || _dbContext.Blogs.Where(b => b.User.Username == User.Identity.Name).FirstOrDefault() != null)
                    {
                        // Validate the fields
                        if (string.IsNullOrEmpty(data.Title))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the title";
                            return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
                        }

                        if (string.IsNullOrEmpty(data.Article))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the article";
                            return View("~/Areas/Blog/Views/Blog/NewPost.cshtml", model);
                        }

                        bool system = (data.BlogId == _config.BlogConfig.ServerBlogId);
                        if (system)
                        {
                            var user = _dbContext.Blogs.Where(b => b.User.Username == User.Identity.Name);
                            if (user != null)
                            {
                                data.BlogId = user.First().BlogId;
                            }
                        }
                        BlogPost post = new BlogPost();
                        post.BlogId = data.BlogId;
                        post.Title = data.Title;
                        post.Article = data.Article;
                        post.System = system;
                        post.DatePosted = DateTime.Now;
                        post.DatePublished = DateTime.Now;
                        post.DateEdited = DateTime.Now;

                        _dbContext.BlogPosts.Add(post);
                        _dbContext.SaveChanges();
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
        public IActionResult EditPost(EditPostViewModel data)
        {
            PostViewModel model = new PostViewModel();
            if (ModelState.IsValid)
            {
                BlogPost post = _dbContext.BlogPosts.Where(p => p.BlogPostId == data.PostId).FirstOrDefault();
                if (post != null)
                {
                    model = new PostViewModel(post);
                    if (User.IsInRole("Admin") || post.Blog.User.Username == User.Identity.Name)
                    {
                        // Validate the fields
                        if (string.IsNullOrEmpty(data.Title))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the title";
                            return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
                        }

                        if (string.IsNullOrEmpty(data.Article))
                        {
                            model.Error = true;
                            model.ErrorMessage = "You must write something for the article";
                            return View("~/Areas/Blog/Views/Blog/EditPost.cshtml", model);
                        }

                        post.Title = data.Title;
                        post.Article = data.Article;
                        post.DateEdited = DateTime.Now;
                        _dbContext.Entry(post).State = EntityState.Modified;
                        _dbContext.SaveChanges();
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
        public IActionResult PublishPost(int postID, bool publish)
        {
            if (ModelState.IsValid)
            {
                BlogPost post = _dbContext.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault();
                if (post != null)
                {
                    if (User.IsInRole("Admin") || post.Blog.User.Username == User.Identity.Name)
                    {
                        post.Published = publish;
                        if (publish)
                            post.DatePublished = DateTime.Now;
                        _dbContext.Entry(post).State = EntityState.Modified;
                        _dbContext.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You are not authorized to publish this post" });
                }
                return Json(new { error = "No post found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public IActionResult DeletePost(int postID)
        {
            if (ModelState.IsValid)
            {
                BlogPost post = _dbContext.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault();
                if (post != null)
                {
                    if (User.IsInRole("Admin") || post.Blog.User.Username == User.Identity.Name)
                    {
                        _dbContext.BlogPosts.Remove(post);
                        _dbContext.SaveChanges();
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
        public IActionResult GetComments(int postID, int startCommentID, int count)
        {
            var comments = _dbContext.BlogPostComments.Where(p => (p.BlogPostId == postID)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
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
        public IActionResult GetCommentArticle(int commentID)
        {
            BlogPostComment comment = _dbContext.BlogPostComments.Where(p => (p.BlogPostCommentId == commentID)).First();
            if (comment != null)
            {
                return Json(new { result = comment.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        public IActionResult CreateComment(int postID, string article)
        {
            if (ModelState.IsValid)
            {
                if (_dbContext.BlogPosts.Where(p => p.BlogPostId == postID).FirstOrDefault() != null)
                {
                    BlogPostComment comment = new BlogPostComment();
                    comment.BlogPostId = postID;
                    comment.UserId = UserHelper.GetUser(_dbContext, User.Identity.Name).UserId;
                    comment.Article = article;
                    comment.DatePosted = DateTime.Now;
                    comment.DateEdited = DateTime.Now;

                    _dbContext.BlogPostComments.Add(comment);
                    _dbContext.SaveChanges();
                    return Json(new { result = true });
                }
                return Json(new { error = "The post does not exist" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public IActionResult EditComment(int commentID, string article)
        {
            if (ModelState.IsValid)
            {
                BlogPostComment comment = _dbContext.BlogPostComments.Where(c => c.BlogPostCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        comment.Article = article;
                        comment.DateEdited = DateTime.Now;
                        _dbContext.Entry(comment).State = EntityState.Modified;
                        _dbContext.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You don't have permission to edit this comment" });
                }
                return Json(new { error = "No comment found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public IActionResult DeleteComment(int commentID)
        {
            if (ModelState.IsValid)
            {
                BlogPostComment comment = _dbContext.BlogPostComments.Where(c => c.BlogPostCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        _dbContext.BlogPostComments.Remove(comment);
                        _dbContext.SaveChanges();
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
