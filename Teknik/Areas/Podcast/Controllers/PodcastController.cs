using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teknik.Areas.Podcast.Models;
using Teknik.Areas.Podcast.ViewModels;
using Teknik.Areas.Users.Utility;
using Teknik.Attributes;
using Teknik.Configuration;
using Teknik.Controllers;
using Teknik.Data;
using Teknik.Filters;
using Teknik.Models;
using Teknik.Utilities;
using Teknik.Logging;

namespace Teknik.Areas.Podcast.Controllers
{
    [Authorize]
    [Area("Podcast")]
    public class PodcastController : DefaultController
    {
        public PodcastController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [ServiceFilter(typeof(TrackPageView))]
        public IActionResult Index()
        {
            MainViewModel model = new MainViewModel();
            model.Title = _config.PodcastConfig.Title;
            model.Description = _config.PodcastConfig.Description;
            try
            {
                ViewBag.Title = _config.PodcastConfig.Title;
                ViewBag.Description = _config.PodcastConfig.Description;
                bool editor = User.IsInRole("Podcast");
                var foundPodcasts = _dbContext.Podcasts.Where(p => (p.Published || editor)).FirstOrDefault();
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
        [ServiceFilter(typeof(TrackPageView))]
        public IActionResult View(int episode)
        {
            PodcastViewModel model = new PodcastViewModel();
            // find the podcast specified
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = _dbContext.Podcasts.Where(p => ((p.Published || editor) && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                model = new PodcastViewModel(foundPodcast);

                ViewBag.Title = model.Title + " | Teknikast";
                return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
            }
            model.Error = true;
            model.ErrorMessage = "No Podcasts Available";
            return View("~/Areas/Podcast/Views/Podcast/ViewPodcast.cshtml", model);
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
        [ServiceFilter(typeof(TrackDownload))]
        public IActionResult Download(int episode, string fileName)
        {
            string path = string.Empty;
            string contentType = string.Empty;
            long contentLength = 0;
            DateTime dateUploaded = new DateTime(1900, 1, 1);

            // find the podcast specified
            var foundPodcast = _dbContext.Podcasts.Where(p => (p.Published && p.Episode == episode)).FirstOrDefault();
            if (foundPodcast != null)
            {
                PodcastFile file = foundPodcast.Files.Where(f => f.FileName == fileName).FirstOrDefault();
                if (file != null)
                {
                    path = file.Path;
                    contentType = file.ContentType;
                    contentLength = file.ContentLength;
                    fileName = file.FileName;
                    dateUploaded = foundPodcast.DateEdited;
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status404NotFound);
                }
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            if (System.IO.File.Exists(path))
            {
                // Are they downloading it by range?
                bool byRange = !string.IsNullOrEmpty(Request.Headers["Range"]); // We do not support ranges

                bool isCached = !string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]); // Check to see if they have a cache

                if (isCached)
                {
                    // The file is cached, let's just 304 this
                    Response.StatusCode = 304;
                    Response.Headers.Add("Content-Length", "0");
                    return Content(string.Empty);
                }
                else
                {
                    long startByte = 0;
                    long endByte = contentLength - 1;
                    long length = contentLength;

                    #region Range Calculation
                    // check to see if we need to pass a specified range
                    if (byRange)
                    {
                        long anotherStart = startByte;
                        long anotherEnd = endByte;
                        string[] arr_split = Request.Headers["Range"].ToString().Split(new char[] { '=' });
                        string range = arr_split[1];

                        // Make sure the client hasn't sent us a multibyte range 
                        if (range.IndexOf(",") > -1)
                        {
                            // (?) Shoud this be issued here, or should the first 
                            // range be used? Or should the header be ignored and 
                            // we output the whole content? 
                            Response.Headers.Add("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                            return new StatusCodeResult(StatusCodes.Status416RequestedRangeNotSatisfiable);
                        }

                        // If the range starts with an '-' we start from the beginning 
                        // If not, we forward the file pointer 
                        // And make sure to get the end byte if spesified 
                        if (range.StartsWith("-"))
                        {
                            // The n-number of the last bytes is requested 
                            anotherStart = startByte - Convert.ToInt64(range.Substring(1));
                        }
                        else
                        {
                            arr_split = range.Split(new char[] { '-' });
                            anotherStart = Convert.ToInt64(arr_split[0]);
                            long temp = 0;
                            anotherEnd = (arr_split.Length > 1 && Int64.TryParse(arr_split[1].ToString(), out temp)) ? Convert.ToInt64(arr_split[1]) : contentLength;
                        }

                        /* Check the range and make sure it's treated according to the specs. 
                         * http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html 
                         */
                        // End bytes can not be larger than $end. 
                        anotherEnd = (anotherEnd > endByte) ? endByte : anotherEnd;
                        // Validate the requested range and return an error if it's not correct. 
                        if (anotherStart > anotherEnd || anotherStart > contentLength - 1 || anotherEnd >= contentLength)
                        {

                            Response.Headers.Add("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                            return new StatusCodeResult(StatusCodes.Status416RequestedRangeNotSatisfiable);
                        }
                        startByte = anotherStart;
                        endByte = anotherEnd;

                        length = endByte - startByte + 1; // Calculate new content length 

                        // Ranges are response of 206
                        Response.StatusCode = 206;
                    }
                    #endregion

                    // We accept ranges
                    Response.Headers.Add("Accept-Ranges", "0-" + contentLength);

                    // Notify the client the byte range we'll be outputting 
                    Response.Headers.Add("Content-Range", "bytes " + startByte + "-" + endByte + "/" + contentLength);

                    // Notify the client the content length we'll be outputting 
                    Response.Headers.Add("Content-Length", length.ToString());

                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = fileName,
                        Inline = true
                    };

                    Response.Headers.Add("Content-Disposition", cd.ToString());

                    FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                    // Reset file stream to starting position (or start of range)
                    fileStream.Seek(startByte, SeekOrigin.Begin);

                    return new BufferedFileStreamResult(contentType, (response) => ResponseHelper.StreamToOutput(response, true, fileStream, (int)length, 4 * 1024), false);
                }
            }
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetPodcasts(int startPodcastID, int count)
        {
            bool editor = User.IsInRole("Podcast");
            var podcasts = _dbContext.Podcasts.Where(p => p.Published || editor).OrderByDescending(p => p.DatePosted).Skip(startPodcastID).Take(count).ToList();
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
        public IActionResult GetPodcastEpisode(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = _dbContext.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Episode });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetPodcastTitle(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = _dbContext.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Title });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetPodcastDescription(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = _dbContext.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                return Json(new { result = foundPodcast.Description });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetPodcastFiles(int podcastId)
        {
            bool editor = User.IsInRole("Podcast");
            var foundPodcast = _dbContext.Podcasts.Where(p => ((p.Published || editor) && p.PodcastId == podcastId)).FirstOrDefault();
            if (foundPodcast != null)
            {
                List<object> files = new List<object>();
                foreach (PodcastFile file in foundPodcast.Files)
                {
                    object fileObj = new
                    {
                        name = file.FileName,
                        id = file.PodcastFileId
                    };
                    files.Add(fileObj);
                }
                return Json(new { result = new { files = files } });
            }
            return Json(new { error = "No podcast found" });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePodcast(int episode, string title, string description)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    // Grab the next episode number
                    Models.Podcast lastPod = _dbContext.Podcasts.Where(p => p.Episode == episode).FirstOrDefault();
                    if (lastPod == null)
                    {
                        // Create the podcast object
                        Models.Podcast podcast = new Models.Podcast();
                        podcast.Episode = episode;
                        podcast.Title = title;
                        podcast.Description = description;
                        podcast.DatePosted = DateTime.Now;
                        podcast.DatePublished = DateTime.Now;
                        podcast.DateEdited = DateTime.Now;
                        podcast.Files = await SaveFilesAsync(Request.Form.Files, episode);

                        _dbContext.Podcasts.Add(podcast);
                        _dbContext.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "That episode already exists" });
                }
                return Json(new { error = "You don't have permission to create a podcast" });
            }
            return Json(new { error = "No podcast created" });
        }

        [HttpPost]
        public async Task<IActionResult> EditPodcast(int podcastId, int episode, string title, string description, string fileIds)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = _dbContext.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault();
                    if (podcast != null)
                    {
                        if (_dbContext.Podcasts.Where(p => p.Episode != episode).FirstOrDefault() == null || podcast.Episode == episode)
                        {
                            podcast.Episode = episode;
                            podcast.Title = title;
                            podcast.Description = description;
                            podcast.DateEdited = DateTime.Now;
                            // Remove any files not in fileIds
                            List<string> fileIdList = new List<string>();
                            if (!string.IsNullOrEmpty(fileIds))
                            {
                                fileIdList = fileIds.Split(',').ToList();
                            }
                            for (int i = 0; i < podcast.Files.Count; i++)
                            {
                                PodcastFile curFile = podcast.Files.ElementAt(i);
                                if (!fileIdList.Exists(id => id == curFile.PodcastFileId.ToString()))
                                {
                                    if (System.IO.File.Exists(curFile.Path))
                                    {
                                        System.IO.File.Delete(curFile.Path);
                                    }
                                    _dbContext.PodcastFiles.Remove(curFile);
                                    podcast.Files.Remove(curFile);
                                }
                            }
                            await SaveFilesAsync(Request.Form.Files, episode);
                            // Add any new files
                            List<PodcastFile> newFiles = await SaveFilesAsync(Request.Form.Files, episode);
                            foreach (PodcastFile file in newFiles)
                            {
                                podcast.Files.Add(file);
                            }

                            // Save podcast
                            _dbContext.Entry(podcast).State = EntityState.Modified;
                            _dbContext.SaveChanges();
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
        public IActionResult PublishPodcast(int podcastId, bool publish)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = _dbContext.Podcasts.Find(podcastId);
                    if (podcast != null)
                    {
                        podcast.Published = publish;
                        if (publish)
                            podcast.DatePublished = DateTime.Now;
                        _dbContext.Entry(podcast).State = EntityState.Modified;
                        _dbContext.SaveChanges();
                        return Json(new { result = true });
                    }
                    return Json(new { error = "No podcast found" });
                }
                return Json(new { error = "You don't have permission to publish this podcast" });
            }
            return Json(new { error = "Invalid Inputs" });
        }

        [HttpPost]
        public IActionResult DeletePodcast(int podcastId)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Podcast"))
                {
                    Models.Podcast podcast = _dbContext.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault();
                    if (podcast != null)
                    {
                        foreach (PodcastFile file in podcast.Files)
                        {
                            System.IO.File.Delete(file.Path);
                        }
                        _dbContext.Podcasts.Remove(podcast);
                        _dbContext.SaveChanges();
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
        public IActionResult GetComments(int podcastId, int startCommentID, int count)
        {
            var comments = _dbContext.PodcastComments.Where(p => (p.PodcastId == podcastId)).OrderByDescending(p => p.DatePosted).Skip(startCommentID).Take(count).ToList();
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
        public IActionResult GetCommentArticle(int commentID)
        {
            PodcastComment comment = _dbContext.PodcastComments.Where(p => (p.PodcastCommentId == commentID)).FirstOrDefault();
            if (comment != null)
            {
                return Json(new { result = comment.Article });
            }
            return Json(new { error = "No article found" });
        }

        [HttpPost]
        public IActionResult CreateComment(int podcastId, string article)
        {
            if (ModelState.IsValid)
            {
                if (_dbContext.Podcasts.Where(p => p.PodcastId == podcastId).FirstOrDefault() != null)
                {
                    PodcastComment comment = new PodcastComment();
                    comment.PodcastId = podcastId;
                    comment.UserId = UserHelper.GetUser(_dbContext, User.Identity.Name).UserId;
                    comment.Article = article;
                    comment.DatePosted = DateTime.Now;
                    comment.DateEdited = DateTime.Now;

                    _dbContext.PodcastComments.Add(comment);
                    _dbContext.SaveChanges();
                    return Json(new { result = true });
                }
                return Json(new { error = "That podcast does not exist" });
            }
            return Json(new { error = "Invalid Parameters" });
        }

        [HttpPost]
        public IActionResult EditComment(int commentID, string article)
        {
            if (ModelState.IsValid)
            {
                PodcastComment comment = _dbContext.PodcastComments.Where(c => c.PodcastCommentId == commentID).FirstOrDefault();
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
                PodcastComment comment = _dbContext.PodcastComments.Where(c => c.PodcastCommentId == commentID).FirstOrDefault();
                if (comment != null)
                {
                    if (comment.User.Username == User.Identity.Name || User.IsInRole("Admin"))
                    {
                        _dbContext.PodcastComments.Remove(comment);
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

        [DisableRequestSizeLimit]
        public async Task<List<PodcastFile>> SaveFilesAsync(IFormFileCollection files, int episode)
        {
            List<PodcastFile> podFiles = new List<PodcastFile>();

            if (files.Count > 0)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i]; 
                                                               
                    long fileSize = file.Length;
                    string fileName = file.FileName;
                    string fileExt = Path.GetExtension(fileName);
                    if (!Directory.Exists(_config.PodcastConfig.PodcastDirectory))
                    {
                        Directory.CreateDirectory(_config.PodcastConfig.PodcastDirectory);
                    }
                    string newName = string.Format("Teknikast_Episode_{0}{1}", episode, fileExt);
                    int index = 1;
                    while (System.IO.File.Exists(Path.Combine(_config.PodcastConfig.PodcastDirectory, newName)))
                    {
                        newName = string.Format("Teknikast_Episode_{0} ({1}){2}", episode, index, fileExt);
                        index++;
                    }
                    string fullPath = Path.Combine(_config.PodcastConfig.PodcastDirectory, newName);
                    PodcastFile podFile = new PodcastFile();
                    podFile.Path = fullPath;
                    podFile.FileName = newName;
                    podFile.ContentType = file.ContentType;
                    podFile.ContentLength = file.Length;
                    podFiles.Add(podFile);

                    using (FileStream fs = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(fs);
                    }
                }
            }

            return podFiles;
        }
    }
}
