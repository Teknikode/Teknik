using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Home.ViewModels;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.Home.Controllers
{
    public class HomeController : DefaultController
    {
        // GET: Home/Home
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            // Grab the latest site blog posts
            List<Post> lastSite = new List<Post>();
            var foundSite = db.Posts.Include("Blog").Include("Blog.User").OrderBy(post => post.DatePosted).Where(p => p.Published && p.System).Take(10);
            if (foundSite != null)
                lastSite = foundSite.ToList();
            // Grab the latest user blog posts
            List<Post> lastPosts = new List<Post>();
            var foundPosts = db.Posts.Include("Blog").Include("Blog.User").OrderBy(post => post.DatePosted).Where(p => p.Published && !p.System).Take(10);
            if (foundPosts != null)
                lastPosts = foundPosts.ToList();
            // Grab the latest podcasts
            List<Post> lastPods = new List<Post>();

            model.SitePosts = lastSite;
            model.Podcasts = lastPods;
            model.BlogPosts = lastPosts;

            ViewBag.Title = Config.Title;
            return View(model);
        }
    }
}