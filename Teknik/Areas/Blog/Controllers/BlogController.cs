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

        // GET: Blogs 
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Title = "Teknik Blog - " + Config.Title;

            // by default, view the teknik blog
            Models.Blog blog = db.Blogs.Find(Constants.SERVERBLOGID);
            BlogViewModel model = new BlogViewModel();
            model.BlogId = Constants.SERVERBLOGID;
            if (blog != null)
            {
                model.UserId = blog.UserId;
                model.User = blog.User;
                model.Posts = blog.Posts;
            }

            return View(model);
        }

        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult Blog(string username)
        {
            Models.Blog blog = null;
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
                                                                                        );
                BlogViewModel model = new BlogViewModel();
                model.BlogId = blog.BlogId;
                model.UserId = blog.UserId;
                model.User = blog.User;
                model.Title = blog.Title;
                model.Description = blog.Description;
                model.Posts = (foundPosts != null && foundPosts.Any()) ? foundPosts.ToList() : null;

                return View(model);
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
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
            var post = db.Posts.Include("Blog").Include("Blog.User").Where(p => (p.Blog.User.Username == username && p.PostId == id) && 
                                                                                (p.Published || p.Blog.User.Username == User.Identity.Name)
                                                                            );
            if (post != null && post.Any())
            {
                Post curPost = post.First();
                PostViewModel model = new PostViewModel(curPost);

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
                                                                            ).Take(count);
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

        // GET: Blogs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BlogId,UserId")] Models.Blog blog)
        {
            if (ModelState.IsValid)
            {
                db.Blogs.Add(blog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blog);
        }

        // GET: Blogs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BlogId,UserId")] Models.Blog blog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(blog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blog);
        }

        // GET: Blogs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Blog blog = db.Blogs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Models.Blog blog = db.Blogs.Find(id);
            db.Blogs.Remove(blog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
