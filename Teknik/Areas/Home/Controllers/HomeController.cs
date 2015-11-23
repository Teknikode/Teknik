using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
            if (db.Posts.Count() > 10)
            {
                lastPosts = db.Posts.Include("Blog").Include("Blog.User").OrderBy(post => post.DatePosted).Take(10).ToList();
            }
            else if (db.Posts.Any())
            {
                lastPosts = db.Posts.Include("Blog").Include("Blog.User").OrderBy(post => post.DatePosted).ToList();
            }

            ViewBag.Title = Config.Title;
            return View(lastPosts);
        }
    }
}