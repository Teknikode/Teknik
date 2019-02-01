using System.Collections.Generic;
using System.Linq;
using Teknik.Areas.Blog.Models;
using Teknik.Areas.Home.ViewModels;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Attributes;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teknik.Logging;
using System;

namespace Teknik.Areas.Home.Controllers
{
    [Authorize]
    [Area("Home")]
    public class HomeController : DefaultController
    {
        public HomeController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public IActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            // Grab the latest site blog posts
            List<BlogPost> lastSite = new List<BlogPost>();

            var foundSite = _dbContext.BlogPosts.Include(p => p.Blog).Include(b => b.Blog.User).OrderByDescending(post => post.DatePosted).Where(p => p.Published && p.System).Take(5);
            if (foundSite != null)
                lastSite = foundSite.ToList();

            // Grab the latest podcasts
            List<Podcast.Models.Podcast> lastPods = new List<Podcast.Models.Podcast>();

            var foundPods = _dbContext.Podcasts.OrderByDescending(post => post.DatePosted).Where(p => p.Published).Take(5);
            if (foundPods != null)
                lastPods = foundPods.ToList();

            model.SitePosts = lastSite;
            model.Podcasts = lastPods;
            
            return View(model);
        }
    }
}