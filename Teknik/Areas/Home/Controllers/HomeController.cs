using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Blog.Models;
using Teknik.Controllers;
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
            List<Post> lastPosts = new List<Post>();
            var found = db.Posts.Include("Blog").Include("Blog.User").OrderBy(post => post.DatePosted).Where(p => p.Published).Take(10);
            if (found != null)
                lastPosts = found.ToList();

            ViewBag.Title = Config.Title;
            return View(lastPosts);
        }
    }
}