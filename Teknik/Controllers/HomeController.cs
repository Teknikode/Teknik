using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Teknik.Models;
using Teknik.ViewModels;

namespace Teknik.Controllers
{
    public class HomeController : DefaultController
    {
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

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Title = Config.Title + " - About";
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Title = Config.Title + " - Contact";
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}