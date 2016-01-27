using MarkdownDeep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Teknik.Areas.Blog.Models;
using Teknik.Controllers;
using Teknik.Helpers;
using Teknik.Models;

namespace Teknik.Areas.RSS.Controllers
{
    public class RSSController : DefaultController
    {
        private TeknikEntities db = new TeknikEntities();

        [AllowAnonymous]
        public ActionResult Index()
        {
            SyndicationFeed feed = new SyndicationFeed("Teknik RSS", "RSS feeds for the Teknik Services.", new Uri(Url.SubRouteUrl("help", "Help.RSS")));

            return new RssResult(feed);
        }

        [AllowAnonymous]
        public ActionResult Blog(string username)
        {
            // If empty, grab the main blog
            Blog.Models.Blog blog = null;
            string blogUrl = Url.SubRouteUrl("blog", "Blog.Blog");
            string title = string.Empty;
            string description = string.Empty;
            bool isSystem = string.IsNullOrEmpty(username);
            if (isSystem)
            {
                blog = db.Blogs.Include("BlogPosts").Include("User").Where(b => b.BlogId == Constants.SERVERBLOGID).FirstOrDefault();
                blogUrl = Url.SubRouteUrl("blog", "Blog.Blog");
            }
            else
            {
                blog = db.Blogs.Include("BlogPosts").Include("User").Where(b => b.User.Username == username).FirstOrDefault();
                blogUrl = Url.SubRouteUrl("blog", "Blog.Blog", new { username = username });
            }
            if (blog != null)
            {
                if (isSystem)
                {
                    title = Config.BlogConfig.Title;
                    description = Config.BlogConfig.Description;
                }
                else
                {
                    title = blog.User.BlogSettings.Title;
                    description = blog.User.BlogSettings.Description;
                }

                List<SyndicationItem> items = new List<SyndicationItem>();

                foreach (BlogPost post in blog.BlogPosts)
                {
                    if (post.Published && post.System == isSystem)
                    {
                        items.Add(new SyndicationItem(
                            post.Title,
                            MarkdownHelper.Markdown(post.Article).ToHtmlString(),
                            new Uri(Url.SubRouteUrl("blog", "Blog.Post", new { username = post.Blog.User.Username, id = post.BlogPostId })),
                            post.BlogPostId.ToString(),
                            post.DateEdited
                            ));
                    }
                }

                SyndicationFeed feed = new SyndicationFeed(title, description, new Uri(blogUrl), items);

                return new RssResult(feed);
            }
            SyndicationFeed badFeed = new SyndicationFeed("No Blog Available", "The specified blog does not exist", new Uri(blogUrl));

            return new RssResult(badFeed);
        }

        [AllowAnonymous]
        public ActionResult Podcast()
        {
            List<SyndicationItem> items = new List<SyndicationItem>();
            List<Podcast.Models.Podcast> podcasts = db.Podcasts.Include("Files").Where(p => p.Published).ToList();
            if (podcasts != null)
            {
                foreach (Podcast.Models.Podcast podcast in podcasts)
                {
                    SyndicationItem item = new SyndicationItem(
                                                    podcast.Title,
                                                    MarkdownHelper.Markdown(podcast.Description).ToHtmlString(),
                                                    new Uri(Url.SubRouteUrl("podcast", "Podcast.View", new { episode = podcast.Episode })),
                                                    podcast.Episode.ToString(),
                                                    podcast.DateEdited
                                                );
                    foreach (Podcast.Models.PodcastFile file in podcast.Files)
                    {
                        SyndicationLink enclosure = SyndicationLink.CreateMediaEnclosureLink(new Uri(Url.SubRouteUrl("podcast", "Podcast.Download", new { episode = podcast.Episode, fileName = file.FileName })), file.ContentType, file.ContentLength);
                        item.Links.Add(enclosure);
                    }

                    items.Add(item);
                }
            }

            SyndicationFeed feed = new SyndicationFeed(Config.PodcastConfig.Title, Config.PodcastConfig.Description, new Uri(Url.SubRouteUrl("podcast", "Podcast.Index")), items);

            return new RssResult(feed);
        }
    }
}