using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Home.ViewModels;
using Teknik.Controllers;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Filters;

namespace Teknik.Areas.Home.Controllers
{
    public class HomeController : DefaultController
    {
        // GET: Home/Home
        private TeknikEntities db = new TeknikEntities();

        [TrackPageView]
        [AllowAnonymous]
        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            // Grab the latest site blog posts
            List<BlogPost> lastSite = new List<BlogPost>();
            if (db.BlogPosts.Count() > 0)
            {
                var foundSite = db.BlogPosts.OrderByDescending(post => post.DatePosted).Where(p => p.Published && p.System).Take(5);
                if (foundSite != null)
                    lastSite = foundSite.ToList();
            }
            // Grab the latest user blog posts
            List<BlogPost> lastPosts = new List<BlogPost>();
            if (db.BlogPosts.Count() > 0)
            {
                var foundPosts = db.BlogPosts.OrderByDescending(post => post.DatePosted).Where(p => p.Published && !p.System).Take(5);
                if (foundPosts != null)
                    lastPosts = foundPosts.ToList();
            }
            // Grab the latest podcasts
            List<Podcast.Models.Podcast> lastPods = new List<Podcast.Models.Podcast>();
            if (db.Podcasts.Count() > 0)
            {
                var foundPods = db.Podcasts.OrderByDescending(post => post.DatePosted).Where(p => p.Published).Take(5);
                if (foundPods != null)
                    lastPods = foundPods.ToList();
            }

            model.SitePosts = lastSite;
            model.Podcasts = lastPods;
            model.BlogPosts = lastPosts;

            ViewBag.Title = Config.Title;
            return View(model);
        }
    }
}