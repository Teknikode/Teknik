using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml.Linq;
using Teknik.Areas.Blog.Models;
using Teknik.Controllers;
using Teknik.Filters;
using Teknik.Utilities;
using Teknik.Models;
using Teknik.Attributes;
using Teknik.Areas.Users.Utility;
using Microsoft.Extensions.Logging;
using Teknik.Configuration;
using Teknik.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SyndicationFeed.Rss;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SyndicationFeed;
using Teknik.Logging;

namespace Teknik.Areas.RSS.Controllers
{
    [TeknikAuthorize(AuthType.Basic)]
    [Area("RSS")]
    public class RSSController : DefaultController
    {
        public RSSController(ILogger<Logger> logger, Config config, TeknikEntities dbContext) : base(logger, config, dbContext) { }
        
        [AllowAnonymous]
        [TrackPageView]
        public async Task Index()
        {
            Response.ContentType = "application/rss+xml";

            using (var xmlWriter = CreateXmlWriter())
            {
                var feedWriter = new RssFeedWriter(xmlWriter);

                await feedWriter.WriteTitle("Teknik RSS");
                await feedWriter.WriteDescription("RSS feeds for the Teknik Services.");
                await feedWriter.Write(new SyndicationLink(new Uri(Url.SubRouteUrl("help", "Help.RSS"))));

                await xmlWriter.FlushAsync();
            }
        }
        
        [AllowAnonymous]
        [TrackPageView]
        public async Task Blog(string username)
        {
            Response.ContentType = "application/rss+xml";

            // If empty, grab the main blog
            List<BlogPost> posts = new List<BlogPost>();

            string blogUrl = Url.SubRouteUrl("blog", "Blog.Blog");
            string title = string.Empty;
            string description = string.Empty;
            bool isSystem = string.IsNullOrEmpty(username);
            bool userExists = false;
            if (isSystem)
            {
                posts = _dbContext.BlogPosts.Where(p => (p.System && p.Published)).ToList();
                blogUrl = Url.SubRouteUrl("blog", "Blog.Blog");
            }
            else
            {
                Blog.Models.Blog blog = _dbContext.Blogs.Where(p => p.User.Username == username && p.BlogId != _config.BlogConfig.ServerBlogId).FirstOrDefault();
                posts = _dbContext.BlogPosts.Where(p => (p.BlogId == blog.BlogId && !p.System) && p.Published).ToList();
                blogUrl = Url.SubRouteUrl("blog", "Blog.Blog", new { username = username });
            }
            if (posts.Any())
            {
                if (isSystem)
                {
                    userExists = true;
                    title = _config.BlogConfig.Title;
                    description = _config.BlogConfig.Description;
                }
                else
                {
                    Users.Models.User user = UserHelper.GetUser(_dbContext, username);
                    if (user != null)
                    {
                        userExists = true;
                        title = user.BlogSettings.Title;
                        description = user.BlogSettings.Description;
                    }
                    else
                    {
                        userExists = false;
                        title = "No Blog Available";
                        description = "The specified user does not exist";
                    }
                }

                List<SyndicationItem> items = new List<SyndicationItem>();

                if (userExists)
                {
                    foreach (BlogPost post in posts.OrderByDescending(p => p.BlogPostId))
                    {
                        if (post.Published && post.System == isSystem)
                        {
                            SyndicationItem item = new SyndicationItem()
                            {
                                Id = post.BlogPostId.ToString(),
                                Title = post.Title,
                                Description = MarkdownHelper.Markdown(post.Article).Value,
                                Published = post.DatePublished
                            };

                            item.AddLink(new SyndicationLink(new Uri(Url.SubRouteUrl("blog", "Blog.Post", new { username = post.Blog.User.Username, id = post.BlogPostId }))));
                            item.AddContributor(new SyndicationPerson(post.Blog.User.Username, UserHelper.GetUserEmailAddress(_config, post.Blog.User.Username)));

                            items.Add(item);
                        }
                    }
                }

                using (var xmlWriter = CreateXmlWriter())
                {
                    var feedWriter = new RssFeedWriter(xmlWriter);

                    await feedWriter.WriteTitle(title);
                    await feedWriter.WriteDescription(description);
                    await feedWriter.Write(new SyndicationLink(new Uri(blogUrl)));
                    
                    foreach (SyndicationItem item in items)
                    {
                        await feedWriter.Write(item);
                    }

                    await xmlWriter.FlushAsync();
                }
            }
            else
            {
                using (var xmlWriter = CreateXmlWriter())
                {
                    var feedWriter = new RssFeedWriter(xmlWriter);

                    await feedWriter.WriteTitle("No Blog Available");
                    await feedWriter.WriteDescription("The specified blog does not exist");
                    await feedWriter.Write(new SyndicationLink(new Uri(blogUrl)));

                    await xmlWriter.FlushAsync();
                }

            }
        }
        
        [AllowAnonymous]
        [TrackPageView]
        public async Task Podcast()
        {
            Response.ContentType = "application/rss+xml";

            List<SyndicationItem> items = new List<SyndicationItem>();
            List<Podcast.Models.Podcast> podcasts = _dbContext.Podcasts.Where(p => p.Published).OrderByDescending(p => p.Episode).ToList();
            if (podcasts != null)
            {
                foreach (Podcast.Models.Podcast podcast in podcasts)
                {
                    SyndicationItem item = new SyndicationItem()
                    {
                        Id = podcast.Episode.ToString(),
                        Title = podcast.Title,
                        Description = MarkdownHelper.Markdown(podcast.Description).Value,
                        Published = podcast.DatePublished
                    };

                    item.AddLink(new SyndicationLink(new Uri(Url.SubRouteUrl("podcast", "Podcast.View", new { episode = podcast.Episode }))));

                    foreach (Podcast.Models.PodcastFile file in podcast.Files)
                    {
                        SyndicationLink enclosure = new SyndicationLink(new Uri(Url.SubRouteUrl("podcast", "Podcast.Download", new { episode = podcast.Episode, fileName = file.FileName })));
                        item.AddLink(enclosure);
                    }

                    items.Add(item);
                }
            }

            using (var xmlWriter = CreateXmlWriter())
            {
                var feedWriter = new RssFeedWriter(xmlWriter);

                await feedWriter.WriteTitle(_config.PodcastConfig.Title);
                await feedWriter.WriteDescription(_config.PodcastConfig.Description);
                await feedWriter.Write(new SyndicationLink(new Uri(Url.SubRouteUrl("podcast", "Podcast.Index"))));

                foreach (SyndicationItem item in items)
                {
                    await feedWriter.Write(item);
                }

                await xmlWriter.FlushAsync();
            }
        }

        private XmlWriter CreateXmlWriter()
        {
            return XmlWriter.Create(Response.Body, new XmlWriterSettings()
            {
                Async = true,
                Encoding = Encoding.UTF8
            });
        }
    }
}