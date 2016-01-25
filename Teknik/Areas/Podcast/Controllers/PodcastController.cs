using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Podcast.ViewModels;
using Teknik.Controllers;
using Teknik.Models;

namespace Teknik.Areas.Podcast.Controllers
{
    public class PodcastController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult Index()
        {
            MainViewModel model = new MainViewModel();
            try
            {
                ViewBag.Title = "Teknik Blog - " + Config.Title;
                var foundPodcasts = (User.IsInRole("Admin")) ? db.Podcasts.FirstOrDefault() : db.Podcasts.Where(p => (p.Published)).FirstOrDefault();
                if (foundPodcasts != null)
                {
                    model = new MainViewModel();
                    model.Title = Config.PodcastConfig.Title;
                    model.Description = Config.PodcastConfig.Description;
                    model.HasPodcasts = (foundPodcasts != null);
                }
                else
                {
                    model.Error = true;
                    model.ErrorMessage = "No Podcasts Available";
                }

                return View("~/Areas/Podcast/Views/Podcast/Main.cshtml", model);
            }
            catch (Exception ex)
            {
                model.Error = true;
                model.ErrorMessage = ex.Message;

                return View("~/Areas/Podcast/Views/Podcast/Main.cshtml", model);
            }
        }

        #region Posts
        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult View(int episode)
        {
            PodcastViewModel model = new PodcastViewModel();
            // find the podcast specified
            var foundPodcast = (User.IsInRole("Admin")) ? db.Podcasts.Where(p => p.Episode == episode).FirstOrDefault() : db.Podcasts.Where(p => (p.Published && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                model.PodcastId = foundPodcast.PodcastId;
                model.Episode = foundPodcast.Episode;
                model.Title = foundPodcast.Title;
                model.Description = foundPodcast.Description;
                model.Files = foundPodcast.Files;

                ViewBag.Title = model.Title + " - Teknikast - " + Config.Title;
                return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "No Podcasts Available";
            return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcasts(int startPodcastID, int count)
        {
            var podcasts = (User.IsInRole("Admin")) ? db.Podcasts.OrderByDescending(p => p.DatePosted).Skip(startPodcastID).Take(count).ToList()
                                                    : db.Podcasts.Where(p => p.Published).OrderByDescending(p => p.DatePosted).Skip(startPodcastID).Take(count).ToList();
            List<PodcastViewModel> podcastViews = new List<PodcastViewModel>();
            if (podcasts != null)
            {
                foreach (Models.Podcast podcast in podcasts)
                {
                    podcastViews.Add(new PodcastViewModel(podcast));
                }
            }
            return PartialView("~/Areas/Podcast/Views/Podcast/Podcasts.cshtml", podcastViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastTitle(int podcastId)
        {
            var foundPodcast = (User.IsInRole("Admin")) ? db.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault() : db.Podcasts.Where(p => (p.Published && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Title });
            }
            return Json(new { error = "No title found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastDescription(int podcastId)
        {
            var foundPodcast = (User.IsInRole("Admin")) ? db.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault() : db.Podcasts.Where(p => (p.Published && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Description });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePodcast(string title, string description, HttpPostedFileBase[] files)
        {
            if (ModelState.IsValid)
            {
                // Handle saving of files
                Models.Podcast podcast = db.Podcasts.Create();
                podcast.Title = title;
                podcast.Description = description;
                podcast.DatePosted = DateTime.Now;
                podcast.DatePublished = DateTime.Now;

                db.Podcasts.Add(podcast);
                db.SaveChanges();
                return Json(new { result = true });
            }
            return Json(new { error = "No podcast created" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPodcast(int podcastId, string title, string description)
        {
            if (ModelState.IsValid)
            {
                Models.Podcast podcast = db.Podcasts.Find(podcastId);
                if (podcast != null)
                {
                    podcast.Title = title;
                    podcast.Description = description;
                    db.Entry(podcast).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PublishPodcast(int podcastId, bool publish)
        {
            if (ModelState.IsValid)
            {
                Models.Podcast podcast = db.Podcasts.Find(podcastId);
                if (podcast != null)
                {
                    podcast.Published = publish;
                    if (publish)
                        podcast.DatePublished = DateTime.Now;
                    db.Entry(podcast).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePodcast(int podcastId)
        {
            if (ModelState.IsValid)
            {
                Models.Podcast podcast = db.Podcasts.Find(podcastId);
                if (podcast != null)
                {
                    db.Podcasts.Remove(podcast);
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
        public ActionResult GetComments(int podcastId, int startCommentID, int count)
        {
            var comments = db.PodcastComments.Include("Podcast").Where(p => (p.PodcastId == podcastId)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
            List<CommentViewModel> commentViews = new List<CommentViewModel>();
            if (comments != null)
            {
                foreach (Models.PodcastComment comment in comments)
                {
                    commentViews.Add(new CommentViewModel(comment));
                }
            }
            return PartialView("~/Areas/Podcast/Views/Podcast/Comments.cshtml", commentViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetCommentArticle(int commentID)
        {
            Models.PodcastComment comment = db.PodcastComments.Include("Podcast").Where(p => (p.PodcastCommentId == commentID)).First();
            if (comment != null)
            {
                return Json(new { result = comment.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateComment(int podcastId, string article)
        {
            if (ModelState.IsValid)
            {
                Models.PodcastComment comment = db.PodcastComments.Create();
                comment.PodcastId = podcastId;
                comment.UserId = db.Users.Where(u => u.Username == User.Identity.Name).First().UserId;
                comment.Article = article;
                comment.DatePosted = DateTime.Now;

                db.PodcastComments.Add(comment);
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
                Models.PodcastComment comment = db.PodcastComments.Find(commentID);
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
                Models.PodcastComment comment = db.PodcastComments.Find(commentID);
                if (comment != null)
                {
                    db.PodcastComments.Remove(comment);
                    db.SaveChanges();
                    return Json(new { result = true });
                }
            }
            return Json(new { error = "No comment found" });
        }
        #endregion
    }
}