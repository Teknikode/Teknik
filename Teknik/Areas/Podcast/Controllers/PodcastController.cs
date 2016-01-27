using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Podcast.ViewModels;
using Teknik.Controllers;
using Teknik.Models;

namespace Teknik.Areas.Podcast.Controllers
{
    public class PodcastController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();
        
        [AllowAnonymous]
        public ActionResult Index()
        {
            MainViewModel model = new MainViewModel();
            model.Title = Config.PodcastConfig.Title;
            model.Description = Config.PodcastConfig.Description;
            try
            {
                ViewBag.Title = "Teknikast - " + Config.Title;
                bool editor = User.IsInRole("Podcast");
                var foundPodcasts = db.Podcasts.Where(p => (p.Published || editor)).FirstOrDefault();
                if (foundPodcasts != null)
                {
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

        #region Podcasts
        [AllowAnonymous]
        public ActionResult View(int episode)
        {
            PodcastViewModel model = new PodcastViewModel();
            // find the podcast specified
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Include("Files").Where(p => ((p.Published || editor) && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                model = new PodcastViewModel(foundPodcast);

                ViewBag.Title = model.Title + " - Teknikast - " + Config.Title;
                return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "No Podcasts Available";
            return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
        }

        [AllowAnonymous]
        public ActionResult Download(int episode, string fileName)
        {
            // find the podcast specified
            var foundPodcast = db.Podcasts.Include("Files").Where(p => (p.Published && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                PodcastFile file = foundPodcast.Files.Where(f => f.FileName == fileName).FirstOrDefault();
                if (file != null)
                {
                    if (System.IO.File.Exists(file.Path))
                    {
                        // Read in the file
                        byte[] data = System.IO.File.ReadAllBytes(file.Path);

                        // Create File
                        var cd = new System.Net.Mime.ContentDisposition
                        {
                            FileName = file.FileName,
                            Inline = true
                        };

                        Response.AppendHeader("Content-Disposition", cd.ToString());

                        return File(data, file.ContentType);
                    }
                }
            }
            return Redirect(Url.SubRouteUrl("error", "Error.Http404"));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcasts(int startPodcastID, int count)
        {
            bool editor = User.IsInRole("Podcast");
            var podcasts = db.Podcasts.Include("Files").Where(p => p.Published || editor).OrderByDescending(p => p.DatePosted).Skip(startPodcastID).Take(count).ToList();
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
        public ActionResult GetPodcastEpisode(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Episode });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastTitle(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Title });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPodcastDescription(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = db.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Description });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePodcast(int episode, string title, string description)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    // Grab the next episode number
                    Models.Podcast lastPod = db.Podcasts.Where(p => p.Episode == episode).FirstOrDefault();
                    if (lastPod == null)
                    {
                        if (Request.Files.Count > 0)
                        {
                            // Create the podcast object
                            Models.Podcast podcast = db.Podcasts.Create();
                            podcast.Episode = episode;
                            podcast.Title = title;
                            podcast.Description = description;
                            podcast.DatePosted = DateTime.Now;
                            podcast.DatePublished = DateTime.Now;
                            podcast.DateEdited = DateTime.Now;

                            // Handle saving of files
                            for (int i = 0; i < Request.Files.Count; i++)
                            {
                                HttpPostedFileBase file = Request.Files[i]; //Uploaded file
                                                                            //Use the following properties to get file's name, size and MIMEType
                                int fileSize = file.ContentLength;
                                string fileName = file.FileName;
                                string fileExt = Path.GetExtension(fileName);
                                if (!Directory.Exists(Config.PodcastConfig.PodcastDirectory))
                                {
                                    Directory.CreateDirectory(Config.PodcastConfig.PodcastDirectory);
                                }
                                string newName = string.Format("Teknikast_Episode_{0}{1}", episode, fileExt);
                                int index = 1;
                                while (System.IO.File.Exists(Path.Combine(Config.PodcastConfig.PodcastDirectory, newName)))
                                {
                                    newName = string.Format("Teknikast_Episode_{0} ({1}){2}", episode, index, fileExt);
                                    index++;
                                }
                                string fullPath = Path.Combine(Config.PodcastConfig.PodcastDirectory, newName);
                                PodcastFile podFile = new PodcastFile();
                                podFile.Path = fullPath;
                                podFile.FileName = newName;
                                podFile.ContentType = file.ContentType;
                                podFile.ContentLength = file.ContentLength;
                                podcast.Files = new List<PodcastFile>();
                                podcast.Files.Add(podFile);

                                file.SaveAs(fullPath);
                            }

                            db.Podcasts.Add(podcast);
                            db.SaveChanges();
                            return Json(new { result = true });
                        }
                        return Json(new { error = "You must submit at least one podcast audio file" });
                    }
                    return Json(new { error = "That episode already exists" });
                }
                return Json(new { error = "You don't have permission to create a podcast" });
            }
            return Json(new { error = "No podcast created" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPodcast(int podcastId, int episode, string title, string description)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = db.Podcasts.Find(podcastId);
                    if (podcast != null)
                    {
                        if (db.Podcasts.Where(p => p.Episode != episode).FirstOrDefault() == null)
                        {
                            podcast.Episode = episode;
                            podcast.Title = title;
                            podcast.Description = description;
                            podcast.DateEdited = DateTime.Now;
                            db.Entry(podcast).State = EntityState.Modified;
                            db.SaveChanges();
                            return Json(new { result = true });
                        }
                        return Json(new { error = "That episode already exists" });
                    }
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to edit this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PublishPodcast(int podcastId, bool publish)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
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
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to publish this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePodcast(int podcastId)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = db.Podcasts.Find(podcastId);
                    if (podcast != null)
                    {
                        db.Podcasts.Remove(podcast);
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to delete this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }
        #endregion

        #region Comments
        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetComments(int podcastId, int startCommentID, int count)
        {
            var comments = db.PodcastComments.Include("BlogPost").Include("BlogPost.Blog").Include("BlogPost.Blog.User").Include("User").Where(p => (p.PodcastId == podcastId)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
            List<CommentViewModel> commentViews = new List<CommentViewModel>();
            if (comments != null)
            {
                foreach (PodcastComment comment in comments)
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
            PodcastComment comment = db.PodcastComments.Where(p => (p.PodcastCommentId == commentID)).FirstOrDefault();
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
                if (db.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault() != null)
                {
                    PodcastComment comment = db.PodcastComments.Create();
                    comment.PodcastId = podcastId;
                    comment.UserId = db.Users.Where(u => u.Username == User.Identity.Name).First().UserId;
                    comment.Article = article;
                    comment.DatePosted = DateTime.Now;
                    comment.DateEdited = DateTime.Now;

                    db.PodcastComments.Add(comment);
                    db.SaveChanges();
                    return Json(new { result = true });
                }
                return Json(new { error = "That podcast does not exist" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditComment(int commentID, string article)
        {
            if (ModelState.IsValid)
            {
                PodcastComment comment = db.PodcastComments.Include("User").Where(c => c.PodcastCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        comment.Article = article;
                        comment.DateEdited = DateTime.Now;
                        db.Entry(comment).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "You don't have permission to edit this comment" });
                }
                return Json(new { error = "No comment found" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int commentID)
        {
            if (ModelState.IsValid)
            {
                PodcastComment comment = db.PodcastComments.Include("User").Where(c => c.PodcastCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        db.PodcastComments.Remove(comment);
                        db.SaveChanges();
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