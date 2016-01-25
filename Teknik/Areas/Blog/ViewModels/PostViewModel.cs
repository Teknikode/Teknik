using System;
using System.Collections.Generic;
using Teknik.Areas.Blog.Models;
using System.Security.Principal;
using System.Linq;
using System.Web;
using Teknik.ViewModels;

namespace Teknik.Areas.Blog.ViewModels
{
    public class PostViewModel : ViewModelBase
    {
        public int PostId { get; set; }

        public int BlogId { get; set; }

        public Models.Blog Blog { get; set; }

        public bool System { get; set; }

        public DateTime DatePosted { get; set; }

        public DateTime DatePublished { get; set; }

        public bool Published { get; set; }

        public string Title { get; set; }

        public string Article { get; set; }

        public List<string> Tags { get; set; }

        public List<BlogPostComment> Comments { get; set; }

        public PostViewModel(BlogPost post)
        {
            BlogId = post.BlogId;
            PostId = post.BlogPostId;
            Blog = post.Blog;
            System = post.System;
            DatePosted = post.DatePosted;
            Published = post.Published;
            DatePublished = post.DatePublished;
            Title = post.Title;
            Tags = post.Tags;
            Article = post.Article;
            Comments = post.Comments;
        }
    }
}